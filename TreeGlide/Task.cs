using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;


namespace TreeGlide
{
    public abstract class Task
    {
        public abstract bool Validate();
        public abstract bool Execute();

        public EntityManager entityManager;
        public LocalPlayer localPlayer;
        public Movement movement;
        public Entity target;
        public Logger logger;

        public Task()
        {
            this.entityManager = MainWindow.entityManager;
            this.localPlayer = MainWindow.localPlayer;
            this.movement = MainWindow.movement;
            this.logger = MainWindow.logger;
        }
    }
}
