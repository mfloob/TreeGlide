using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeGlide.Managers;

namespace TreeGlide
{
    public class GrindBot : TaskManager
    {
        public override void OnStart()
        {
            Add(new AttackTarget(), new MoveToNearestEnemy(), new StuckCheck(), new MoveAlongPath());
        }

        public GrindBot(PathManager pathManager)
        {
            OnStart();
            pathManager.Initialize();
            logger.Log("Grind bot started.");
        }
    }

    internal class MoveAlongPath : Task
    {
        public override bool Validate()
        {
            target = entityManager.GetTarget(140f);
            return target == null;
        }

        public override bool Execute()
        {
            pathManager.MoveAlongPath();
            return true;
        }

    }

    internal class AttackTarget : Task
    {
        public override bool Validate()
        {
            target = entityManager.GetTarget(140f);
            return target != null && target.GetDistance() <= 30f && target.IsAlive();
        }

        public override bool Execute()
        {
            movement.Attack();
            return true;
        }
    }

    internal class MoveToNearestEnemy : Task
    {
        public override bool Validate()
        {
            target = entityManager.GetTarget(140f);
            if (target?.GetDistance() <= 30f)
            {
                movement.KeysUp();
                return false;
            }
            return target != null && target.IsAlive();
        }

        public override bool Execute()
        {
            if (target != null && target?.GetDistance() > 30f)
                movement.MoveToPoint(new PointF(target.xCoord, target.yCoord), 30f);
            Console.WriteLine("Moving to enemy");
            return true;
        }
    }

    internal class StuckCheck : Task
    {
        private PointF oldCoords;
        private int oldEnemyHealth;
        private int moveCount;
        private int attackCount;

        public override bool Validate()
        {
            target = entityManager.GetTarget(140f);
            if (target == null)
            {
                movement.AttackUp();
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

            if (moveCount > 5 || attackCount > 5)
            {
                moveCount = 0;
                attackCount = 0;
                return true;
            }

            return false;
        }

        public override bool Execute()
        {
            PointF newPoint = movement.PointOnVector(new PointF(target.xCoord, target.yCoord), 30f);
            localPlayer.Teleport(newPoint.X, newPoint.Y, target.zCoord);
            return true;
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
