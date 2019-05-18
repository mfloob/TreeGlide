using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeTree
{
    public class LocalPlayer
    {
        private int health;
        private float xCoord;
        private float yCoord;
        private float zCoord;
        private float attackSpeed;
        private float movementSpeed;
        MemoryManager memoryManager;

        #region Offsets
        private struct Offsets
        {            
            internal const int LOCALPLAYER_1 = 0x8;
            internal const int LOCALPLAYER_2 = 0x8C;
            internal const int HEALTH = 0x428;
            internal const int COORDS_X = 0x4E4;
            internal const int COORDS_Y = 0x4EC;
            internal const int COORDS_Z = 0x4E8;
            internal const int ATTACKSPEED = 0x7DC;
            internal const int MOVESPEED = 0x560;
        }
        #endregion

        public LocalPlayer(MemoryManager memoryManager)
        {
            this.memoryManager = memoryManager;
            this.UpdateValues();
        }

        public void UpdateValues()
        {
            this.health = GetHealth();
            this.xCoord = GetX();
            this.yCoord = GetY();
            this.zCoord = GetZ();
            this.attackSpeed = this.memoryManager.ReadValue<float>(new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.ATTACKSPEED });
            this.movementSpeed = this.memoryManager.ReadValue<float>(new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.MOVESPEED });
        }

        public int GetHealth()
        {
            this.health = this.memoryManager.ReadValue<int>(new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.HEALTH });
            return this.health;
        }

        public void Teleport(float x, float y, float z)
        {
            memoryManager.WriteValue(x, new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.COORDS_X });
            memoryManager.WriteValue(y, new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.COORDS_Y });
            memoryManager.WriteValue(z, new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.COORDS_Z });
        }

        public float GetX()
        {
            this.xCoord = this.memoryManager.ReadValue<float>(new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.COORDS_X });
            return this.xCoord;
        }
        public float GetY()
        {
            this.yCoord = this.memoryManager.ReadValue<float>(new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.COORDS_Y });
            return this.yCoord;
        }
        public float GetZ()
        {
            this.zCoord = this.memoryManager.ReadValue<float>(new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.COORDS_Z });
            return this.zCoord;
        }

    }
}
