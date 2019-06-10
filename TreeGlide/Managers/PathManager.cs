using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TreeGlide.Managers
{
    public class PathManager
    {
        public class Checkpoint
        {
            public int id;
            public float X { get; private set; }
            public float Y { get; private set; }
            public float Z { get; private set; }
            public float distanceFromMe;
            public bool traversed = false;
            public Checkpoint child;
            public Checkpoint parent;

            public Checkpoint()
            {
            }

            public Checkpoint(float x, float y, float z)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }

            public Checkpoint(float x, float y, float z, int id, Checkpoint parent)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
                this.id = id;
                this.parent = parent;
            }

            public override string ToString()
            {
                return String.Format("[{0}, {1}, {2}, {3}, {4}]", this.X.ToString("F4"), this.Y.ToString("F4"), this.Z.ToString("F4"), this.id, !(this.parent==null) ? parent.id : 0);
            }

            public bool ConnectedWith(Checkpoint checkpoint)
            {
                return (this.child == checkpoint || this.parent == checkpoint);
            }

            public float DistanceFromMe(LocalPlayer localPlayer)
            {
                float localX = localPlayer.X;
                float localY = localPlayer.Y;
                float localZ = localPlayer.Z;
                float deltaX = this.X - localX;
                float deltaY = this.Y - localY;
                float deltaZ = this.Z - localZ;
                this.distanceFromMe = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
                return this.distanceFromMe;
            }
        }

        public class Path
        {
            public List<Checkpoint> checkpointList;
            private Logger logger;
            private LocalPlayer localPlayer;
            private string name;

            public Path(LocalPlayer localPlayer, Logger logger)
            {
                this.logger = logger;
                this.localPlayer = localPlayer;
                this.checkpointList = new List<Checkpoint>();
            }
            public Path(List<Checkpoint> checkpoints, LocalPlayer localPlayer, string name)
            {
                this.checkpointList = checkpoints;
                this.name = name;
                this.localPlayer = localPlayer;
            }
            public string SetName(string name) => this.name = name;
            public string Name => this.name;

            public void Add(int id)
            {
                var checkpoint = new Checkpoint();
                if (this.checkpointList.Count == 0)
                {
                    checkpoint = new Checkpoint(localPlayer.X, localPlayer.Y, localPlayer.Z);
                    checkpoint.id = id;
                }
                else
                    checkpoint = new Checkpoint(localPlayer.X, localPlayer.Y, localPlayer.Z, id, checkpointList[checkpointList.Count - 1]);
                checkpointList.Add(checkpoint);
                logger.Log(String.Format("New checkpoint at: {0}.", checkpoint));
            }

            public Checkpoint ClosestCheckpoint()
            {
                Checkpoint closest = null;
                float closestDistance = 999999;

                foreach (Checkpoint checkpoint in this.checkpointList)
                {
                    checkpoint.distanceFromMe = checkpoint.DistanceFromMe(localPlayer);
                    if (checkpoint.distanceFromMe < closestDistance)
                    {
                        closestDistance = checkpoint.distanceFromMe;
                        closest = checkpoint;
                    }
                }
                return closest;
            }            

        }


        private LocalPlayer localPlayer;
        private TimerManager timerManager;
        private Movement movement;
        public Logger logger;
        private bool running;
        private Path createdPath;
        private Path currentPath;
        private Checkpoint destination;
        private Checkpoint currentPoint;
        private bool backwards = false;
        

        private async Task<T> Run<T>(T x) => await System.Threading.Tasks.Task.Run(() => x);
        public PathManager(TimerManager timerManager, LocalPlayer localPlayer, Movement movement, ItemsControl logBox)
        {
            this.timerManager = timerManager;
            this.localPlayer = localPlayer;
            this.movement = movement;
            this.logger = new Logger(logBox);
            logger.Log("PathManager initialized.");
        }      

        public void StartCreatePath()
        {
            if (running)
                return;
            Path path = new Path(localPlayer, logger);
            this.createdPath = path;
            DispatcherTimer pathTimer = timerManager.CreateTimer(250, true);
            pathTimer.Tick += (s, e1) => { pathTimer_Tick(s, e1, path); };
            logger.Log("Creating path... Move in any direction to add checkpoints.");
            pathTimer.Start();
            this.running = true;
        }

        public void StopCreatePath()
        {
            timerManager.StopTimers();
            logger.Log("Path has been generated. Press Save to save this path.");
            this.running = false;
        }

        public async void SavePath(string name)
        {
            string assemblyPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string directory = Directory.CreateDirectory(assemblyPath + "/Paths").ToString();
            StreamWriter outputFile;

            using (outputFile = new StreamWriter(System.IO.Path.Combine(directory, name + ".txt")))
            {
                foreach (Checkpoint checkpoint in this.createdPath.checkpointList)
                    await outputFile.WriteLineAsync(checkpoint.ToString());
            }
            logger.Log("Path saved in:\n" + System.IO.Path.GetFullPath(directory));
            this.createdPath.SetName(name);
        }

        private async void pathTimer_Tick(object sender, EventArgs e, Path path)
        {
            if (!MainWindow.localFound)
                return;            
            var checkpointList = await Run(path.checkpointList);
            int id = checkpointList.Count + 1;
            if (checkpointList.Count == 0)
            {
                path.Add(id++);
                return;
            }
            if (checkpointList[checkpointList.Count - 1].DistanceFromMe(localPlayer) < 50f)    //Make distance selectable
                return;
            path.Add(id++);
        }

        public async void SetPath(string name)
        {
            string assemblyPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string directory = (Directory.CreateDirectory(assemblyPath + "/Paths").ToString() + "/" + name + ".txt");
            var path = new Path(new List<Checkpoint>(), localPlayer, name);
            this.currentPath = path;

            using (var stream = new StreamReader(directory))
            {
                string line;
                while ((line = await stream.ReadLineAsync()) != null)
                    path.checkpointList.Add(ConvertCheckpoint(line));
            }

            if (path.checkpointList == null)
            {
                logger.Log("Error: Selected path checkpointList is null.");
                return;
            }
            logger.Log(name + " successfully loaded.");
            SetChildren(this.currentPath);
        }

        public Checkpoint ConvertCheckpoint(string line)
        {
            var matches = Regex.Matches(line, @"([-+]?[0-9]*\.?[0-9]+)");
            if (matches.Count != 5)
                return null;
            return new Checkpoint(Convert.ToSingle(matches[0].Value), Convert.ToSingle(matches[1].Value), Convert.ToSingle(matches[2].Value), Convert.ToInt32(matches[3].Value), FindById(Convert.ToInt32(matches[4].Value)));
        }

        public Checkpoint FindById(int id)
        {
            foreach (Checkpoint checkpoint in currentPath.checkpointList)
            {
                if (checkpoint.id == id)
                    return checkpoint;
            }
            return null;
        }

        public void SetChildren(Path currentPath)
        {
            foreach (Checkpoint parent in currentPath.checkpointList)
            {
                foreach (Checkpoint child in currentPath.checkpointList)
                {
                    if (child.parent == parent)
                    {
                        parent.child = child;
                        continue;
                    }
                }
            }
        }

        public void Initialize()
        {
            //currentPoint = currentPath.ClosestCheckpoint();
            //SetDestinationInOrder();
        }

        private bool SetDestinationInOrder()
        {
            if (currentPoint.id == currentPath.checkpointList.Count && destination != currentPath.checkpointList[currentPoint.id - 2])
                backwards = true;
            else if (currentPoint.id == currentPath.checkpointList[0].id && destination != currentPath.checkpointList[currentPoint.id])
                backwards = false;
            destination = backwards ? currentPath.checkpointList[currentPoint.id - 2] : currentPath.checkpointList[currentPoint.id];
            if (destination != null)
                return true;
            return false;
        }

        public void MoveAlongPath()
        {
            if (destination.DistanceFromMe(localPlayer) <= 40f)
                currentPoint = destination;
            if (!SetDestinationInOrder())
                return;
            if (currentPoint.ConnectedWith(destination))
            {
                movement.MoveToPoint(destination, 40f);
                return;
            }
        }

        private void NavigateConnected(Checkpoint destination)
        {
            if (destination.DistanceFromMe(localPlayer) <= 40f)
            {
                currentPoint = destination;
                return;
            }
            
        }
    }
}
