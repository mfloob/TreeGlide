using System;
using System.Collections.Generic;
using System.Linq;
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
            public Path(LocalPlayer localPlayer, Logger logger)
            {
                this.logger = logger;
                this.localPlayer = localPlayer;
                this.checkpointList = new List<Checkpoint>();
            }

            public void Add()
            {
                Checkpoint checkpoint = new Checkpoint(localPlayer.GetX(), localPlayer.GetY(), localPlayer.GetZ());
                checkpointList.Add(checkpoint);
                logger.Log(String.Format("Added checkpoint at: {0}.", checkpoint.ToString()));
            }

            public List<Checkpoint> GetCheckpointsList()
            {
                return checkpointList;
            }

            public Checkpoint ClosestCheckpoint()
            {
                Checkpoint closest = null;
                float closestDistance = 999999;
                float localX = localPlayer.GetX();
                float localY = localPlayer.GetY();
                float localZ = localPlayer.GetZ();

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

        }

        private LocalPlayer localPlayer;
        private TimerManager timerManager;
        private Logger logger;
        private async Task<T> Run<T>(T x) => await System.Threading.Tasks.Task.Run(() => x);
        public PathManager(TimerManager timerManager, LocalPlayer localPlayer, ItemsControl logBox)
        {
            this.timerManager = timerManager;
            this.localPlayer = localPlayer;
            this.logger = new Logger(logBox);
            logger.Log("Pathmanager initialized.");
        }      

        public void StartCreatePath()
        {
            if (!MainWindow.localFound)
                return;
            Path path = new Path(localPlayer, logger);
            DispatcherTimer pathTimer = timerManager.CreateTimer(50, true);
            pathTimer.Tick += (s, e1) => { pathTimer_Tick(s, e1, path); };
            logger.Log("Creating path... Move in any direction to add checkpoints.");
            pathTimer.Start();
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
            var closestCheckPointDistance = await Run(path.ClosestCheckpoint().distanceFromMe);
            if (closestCheckPointDistance < 150f)    //Make distance selectable
                return;
            path.Add();
        }
    }
}
