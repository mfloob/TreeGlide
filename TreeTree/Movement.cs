using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeTree
{
    public class Movement
    {
        private LocalPlayer localPlayer;
        private MemoryManager memoryManager;
        public float xCoord;
        public float yCoord;
        public float zCoord;

        public Movement(LocalPlayer localPlayer)
        {
            this.localPlayer = localPlayer;
            this.memoryManager = MainWindow.memoryManager;
        }

        private void GetPlayerCoords()
        {
            this.xCoord = localPlayer.GetX();
            this.yCoord = localPlayer.GetY();
            this.zCoord = localPlayer.GetZ();
        }
        
        public PointF MyCoordsToPoint()
        {
            GetPlayerCoords();
            return new PointF(this.xCoord, this.yCoord);
        }

        public double MyAngleToPoint(PointF point)
        {
            PointF myPosition = MyCoordsToPoint();
            double angle = Math.Atan2((myPosition.Y - point.Y), (myPosition.X - point.X));
            angle = angle * 180 / Math.PI;
            if (angle < 0)
            {
                angle = 180 + (180 + angle);
            }
            return angle;
        }

        public double MyDistanceToPoint(PointF point)
        {
            PointF point2 = MyCoordsToPoint();
            double a = (double)(point.X - point2.X);
            double b = (double)(point.Y - point2.Y);

            return Math.Sqrt(a * a + b * b);
        }

        public PointF PointOnVector(PointF point, float distance)
        {
            PointF destination = new PointF();
            destination.X = (float) (localPlayer.GetX() + distance * Math.Cos(MyAngleToPoint(point)));
            destination.Y = (float)(localPlayer.GetY() + distance * Math.Sin(MyAngleToPoint(point)));

            return destination;
        }

        public void MoveToPoint(PointF destination, float maxDistance)
        {
            double angle = MyAngleToPoint(destination);
            double distance = MyDistanceToPoint(destination);

            if (distance < maxDistance)
            {
                KeysUp();
                return;
            }
            //InputManager.CastKeyUp(space);
            if (angle > 22.5 && angle <= 67.5)
                MoveLeft();
            else if (angle > 67.5 && angle <= 112.5)
                MoveDownLeft();
            else if (angle > 112.5 && angle <= 157.5)
                MoveDown();
            else if (angle > 157.5 && angle <= 202.5)
                MoveDownRight();
            else if (angle > 202.5 && angle <= 247.5)
                MoveRight();
            else if (angle > 247.5 && angle <= 292.5)
                MoveUpRight();
            else if (angle > 292.5 && angle <= 337.5)
                MoveUp();
            else if (angle > 337.5 || angle <= 22.5)
                MoveUpLeft();
            else
                Console.WriteLine("Invalid angle: " + MyAngleToPoint(destination));
            return;
        }

        public void Attack()
        {
            KeysUp();
            InputManager.CastKey(space);
        }        

        #region Move Utilities
        readonly short up = InputManager.ScanCodes.W;
        readonly short down = InputManager.ScanCodes.S;
        readonly short left = InputManager.ScanCodes.A;
        readonly short right = InputManager.ScanCodes.D;
        readonly short space = InputManager.ScanCodes.SPACE;
        private void MoveUp()
        {
            InputManager.CastKeyUp(up);
            InputManager.CastKeyUp(down);
            InputManager.CastKeyUp(right);
            InputManager.CastKeyDown(up);
        }
        private void MoveDown()
        {
            InputManager.CastKeyUp(up);
            InputManager.CastKeyUp(left);
            InputManager.CastKeyUp(right);
            InputManager.CastKeyDown(down);
        }
        private void MoveLeft()
        {
            InputManager.CastKeyUp(up);
            InputManager.CastKeyUp(down);
            InputManager.CastKeyUp(right);
            InputManager.CastKeyDown(left);
        }
        private void MoveRight()
        {
            InputManager.CastKeyUp(up);
            InputManager.CastKeyUp(left);
            InputManager.CastKeyUp(down);
            InputManager.CastKeyDown(right);
        }
        private void MoveUpRight()
        {
            InputManager.CastKeyUp(left);
            InputManager.CastKeyUp(down);
            InputManager.CastKeyDown(up);
            InputManager.CastKeyDown(right);
        }
        private void MoveUpLeft()
        {
            InputManager.CastKeyUp(down);
            InputManager.CastKeyUp(right);
            InputManager.CastKeyDown(up);
            InputManager.CastKeyDown(left);
        }
        private void MoveDownRight()
        {
            InputManager.CastKeyUp(up);
            InputManager.CastKeyUp(left);
            InputManager.CastKeyDown(down);
            InputManager.CastKeyDown(right);
        }
        private void MoveDownLeft()
        {
            InputManager.CastKeyUp(up);
            InputManager.CastKeyUp(right);
            InputManager.CastKeyDown(down);
            InputManager.CastKeyDown(left);
        }
        private void KeysUp()
        {
            InputManager.CastKeyUp(up);
            InputManager.CastKeyUp(right);
            InputManager.CastKeyUp(down);
            InputManager.CastKeyUp(left);
        }
        #endregion
    }
}
