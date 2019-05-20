using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TreeGlide.Managers
{
    public class PathManager
    {
        internal class Checkpoint
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

        internal class Path
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

            public void Add()
            {
                Checkpoint checkpoint = new Checkpoint(localPlayer.X, localPlayer.Y, localPlayer.Z);
                checkpointList.Add(checkpoint);
                logger.Log(String.Format("New checkpoint at: {0}.", checkpoint));
            }

            public List<Checkpoint> GetCheckpointsList()
            {
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
        private Logger logger;
        private bool running;
        private Path currentPath;
        private List<Path> pathList;

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
            DispatcherTimer pathTimer = timerManager.CreateTimer(75, true);
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

        //public List<Path> GetPaths()
        //{
        //    string assemblyPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //    string directory = Directory.CreateDirectory(assemblyPath + "/Paths").ToString();
            
        //    foreach (Path path in GeneratePaths())
        //    {
        //        pathList.Add(path);
        //    }
        //}
    }
}
