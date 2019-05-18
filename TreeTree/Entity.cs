using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeTree
{
    public class Entity
    {
        public int mem, id, health;
        public float xCoord, yCoord, zCoord;
        public bool isAlive;
        private MemoryManager memoryManager;
        private Movement movement;

        #region Offsets
        private struct Offsets
        {
            internal const int ENTITY_BASE_1 = 0x4;
            internal const int ENTITY_BASE_2 = 0x90;
            internal const int ENTITY_ID = 0x3DC;
            internal const int ENTITY_HEALTH = 0x428;
            internal const int ENTITY_X = 0x4E4;
            internal const int ENTITY_Y = 0x4EC;
            internal const int ENTITY_Z = 0x4E8;
        }
        #endregion

        public Entity(int mem, MemoryManager memoryManager)
        {
            this.mem = mem;
            this.memoryManager = memoryManager;
            this.movement = MainWindow.movement;
            this.id = GetID();
        }

        public void UpdateValues()
        {
            this.health = GetHealth();
            this.xCoord = GetX();
            this.yCoord = GetY();
            this.zCoord = GetZ();
            this.isAlive = IsAlive();
        }

        public int GetHealth()
        {
            return this.memoryManager.ReadValue<int>(new int[] { Offsets.ENTITY_BASE_1, Offsets.ENTITY_BASE_2, this.mem, Offsets.ENTITY_HEALTH });
        }
        public int GetID()
        {
            return this.memoryManager.ReadValue<int>(new int[] { Offsets.ENTITY_BASE_1, Offsets.ENTITY_BASE_2, this.mem, Offsets.ENTITY_ID });
        }

        public float GetX()
        {
            return this.memoryManager.ReadValue<float>(new int[] { Offsets.ENTITY_BASE_1, Offsets.ENTITY_BASE_2, this.mem, Offsets.ENTITY_X });
        }
        public float GetY()
        {
            return this.memoryManager.ReadValue<float>(new int[] { Offsets.ENTITY_BASE_1, Offsets.ENTITY_BASE_2, this.mem, Offsets.ENTITY_Y });
        }
        public float GetZ()
        {
            return this.memoryManager.ReadValue<float>(new int[] { Offsets.ENTITY_BASE_1, Offsets.ENTITY_BASE_2, this.mem, Offsets.ENTITY_Z });
        }
        public bool IsAlive()
        {
            return this.GetHealth() > 0;
        }
        public double GetDistance()
        {
            return movement.MyDistanceToPoint(new PointF(this.xCoord, this.yCoord));
        }
    }
}
