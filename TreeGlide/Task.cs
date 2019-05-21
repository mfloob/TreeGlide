using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using TreeGlide.Managers;

namespace TreeGlide
{
    public abstract class Task
    {
        public abstract bool Validate();
        public abstract bool Execute();

        public EntityManager entityManager;
        public LocalPlayer localPlayer;
        public PathManager pathManager;
        public Movement movement;
        public Entity target;
        public Logger logger;

        public Task()
        {
            this.entityManager = MainWindow.entityManager;
            this.localPlayer = MainWindow.localPlayer;
            this.pathManager = MainWindow.pathManager;
            this.movement = MainWindow.movement;
            this.logger = MainWindow.logger;
        }
    }
}
