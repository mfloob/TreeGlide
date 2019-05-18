using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TreeTree
{
    public abstract class Task
    {
        public abstract bool Validate();
        public abstract void Execute();

        public EntityManager entityManager;
        public LocalPlayer localPlayer;
        public Movement movement;
        public Entity target;

        public Task()
        {
            this.entityManager = MainWindow.entityManager;
            this.localPlayer = MainWindow.localPlayer;
            this.movement = MainWindow.movement;
        }
    }

    public class AttackTarget : Task
    {
        public override bool Validate()
        {
            target = entityManager.NearestEntity();
            if (target == null)
                return false;            
            return target.GetDistance() < 20f;
        }

        public override void Execute()
        {            
            movement.Attack();
        }
    }

    public class MoveToNearestEnemy : Task
    {
        public override bool Validate()
        {
            target = entityManager.NearestEntity();
            if (target == null)
                return false;
            if (target.GetDistance() >= 20f)
            {
                return true;
            }
            return false;
        }

        public override void Execute()
        {
            movement.MoveToPoint(new PointF(target.xCoord, target.yCoord), 20f);
        }
    }

    public class StuckCheck : Task
    {
        private PointF oldCoords;
        private int oldEnemyHealth;
        private int moveCount;
        private int attackCount;

        public override bool Validate()
        {
            target = entityManager.NearestEntity();
            if (target == null)
            {
                Console.WriteLine("Target is Null");
                return false;
            }

            bool moving = Moving();
            bool attacking = Attacking();

            if ((moving && !attacking) || (!moving && attacking))
            {
                moveCount = attackCount = 0;
                return false;
            }

            if (!moving)
                moveCount++;

            else if (!attacking)
                attackCount++;

            if (moveCount > 10 || attackCount > 10)
                return true;

            return false;
        }

        public override void Execute()
        {
            PointF newPoint = movement.PointOnVector(new PointF(target.xCoord, target.yCoord), 30f);
            localPlayer.Teleport(newPoint.X, newPoint.Y, target.zCoord);
        }

        private bool Moving()
        {
            PointF newCoords = movement.MyCoordsToPoint();
            if (!(newCoords == oldCoords))
            {
                oldCoords = newCoords;
                return true;
            }
            return false;
        }

        private bool Attacking()
        {
            int newEnemyHealth = target.health;
            if (!(newEnemyHealth == oldEnemyHealth))
            {
                oldEnemyHealth = newEnemyHealth;
                return true;
            }
            return false;
        }
    }
}
