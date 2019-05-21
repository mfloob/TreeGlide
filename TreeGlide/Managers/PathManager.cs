using System;
using System.Collections.Generic;
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
            public float x, y, z, distanceFromMe;
            public Checkpoint(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public override string ToString()
            {
                return String.Format("[{0}, {1}, {2}]", this.x.ToString("F4"), this.y.ToString("F4"), this.z.ToString("F4"));
            }
        }

        public class Path
        {

            private List<Checkpoint> checkpointList;
            private Logger logger;
            private LocalPlayer localPlayer;
            private string name;
            public Path(LocalPlayer localPlayer, Logger logger)
            {
                this.logger = logger;
                this.localPlayer = localPlayer;
                this.checkpointList = new List<Checkpoint>();
            }
            public Path(List<Checkpoint> checkpoints, string name)
            {
                this.checkpointList = checkpoints;
                this.name = name;
            }

            public void Add()
            {
                Checkpoint checkpoint = new Checkpoint(localPlayer.X, localPlayer.Y, localPlayer.Z);
                checkpointList.Add(checkpoint);
                logger.Log(String.Format("New checkpoint at: {0}.", checkpoint));
            }

            public List<Checkpoint> GetCheckpointsList()
            {
                foreach (Checkpoint checkpoint in this.checkpointList)
                {
                    if (checkpoint == null)
                        return null;
                }
                return checkpointList;
            }

            public Checkpoint ClosestCheckpoint()
            {
                Checkpoint closest = null;
                float closestDistance = 999999;
                float localX = localPlayer.X;
                float localY = localPlayer.Y;
                float localZ = localPlayer.Z;

                foreach (Checkpoint checkpoint in this.checkpointList)
                {
                    float deltaX = checkpoint.x - localX;
                    float deltaY = checkpoint.y - localY;
                    float deltaZ = checkpoint.z - localZ;
                    checkpoint.distanceFromMe = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
                    if (checkpoint.distanceFromMe < closestDistance)
                    {
                        closestDistance = checkpoint.distanceFromMe;
                        closest = checkpoint;
                    }
                }
                return closest;
            }
            public string SetName(string name) => this.name = name;
            public string GetName() => this.name;

        }

        private LocalPlayer localPlayer;
        private TimerManager timerManager;
        public Logger logger;
        private bool running;
        private Path currentPath;
        private Path selectedPath;
        //private List<Path> pathList;

        private async Task<T> Run<T>(T x) => await System.Threading.Tasks.Task.Run(() => x);
        public PathManager(TimerManager timerManager, LocalPlayer localPlayer, ItemsControl logBox)
        {
            this.timerManager = timerManager;
            this.localPlayer = localPlayer;
            this.logger = new Logger(logBox);
            logger.Log("PathManager initialized.");
        }      

        public void StartCreatePath()
        {
            if (running)
                return;
            Path path = new Path(localPlayer, logger);
            this.currentPath = path;
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
                foreach (Checkpoint checkpoint in this.currentPath.GetCheckpointsList())
                    await outputFile.WriteLineAsync(checkpoint.ToString());
            }
            logger.Log("Path saved in:\n" + System.IO.Path.GetFullPath(directory));
            this.currentPath.SetName(name);
        }

        private async void pathTimer_Tick(object sender, EventArgs e, Path path)
        {
            if (!MainWindow.localFound)
                return;
            var checkpointList = await Run(path.GetCheckpointsList());
            if (checkpointList.Count == 0)
            {
                path.Add();
                return;
            }
            var closestCheckpointDistance = await Run(path.ClosestCheckpoint().distanceFromMe);
            if (closestCheckpointDistance < 150f)    //Make distance selectable
                return;
            path.Add();
        }

        public async void SetPath(string name)
        {
            string assemblyPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string directory = (Directory.CreateDirectory(assemblyPath + "/Paths").ToString() + "/" + name + ".txt");
            var checkpoints = new List<Checkpoint>();

            using (var stream = new StreamReader(directory))
            {
                string line;
                while ((line = await stream.ReadLineAsync()) != null)
                    checkpoints.Add(ConvertCheckpoint(line));
            }

            var path = new Path(checkpoints, name);
            if (path.GetCheckpointsList() == null)
            {
                logger.Log("Error: Selected path checkpointList is null.");
                return;
            }
            logger.Log(name + " successfully loaded.");
            this.selectedPath = path;
        }

        public Checkpoint ConvertCheckpoint(string line)
        {
            var matches = Regex.Matches(line, @"([-+]?[0-9]*\.?[0-9]+)");
            if (matches.Count != 3)
                return null;
            var checkpoint = new Checkpoint(Convert.ToSingle(matches[0].Value), Convert.ToSingle(matches[1].Value), Convert.ToSingle(matches[2].Value));

            return checkpoint;
        }
    }
}
