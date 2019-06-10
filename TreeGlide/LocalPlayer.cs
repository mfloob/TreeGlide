using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeGlide
{
    public class LocalPlayer
    {
        private int health;
        public float X;
        public float Y;
        public float Z;
        private float attackSpeed;
        private float movementSpeed;
        private float faceAngle;
        private bool isFound;
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
            internal const int FACEANGLE = 0x558;
        }
        #endregion

        public LocalPlayer(MemoryManager memoryManager)
        {
            this.memoryManager = memoryManager;
        }

        public void UpdateValues()
        {
            this.health = GetHealth();
            this.X = GetX();
            this.Y = GetY();
            this.Z = GetZ();
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
            this.X = this.memoryManager.ReadValue<float>(new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.COORDS_X });
            return this.X;
        }
        public float GetY()
        {
            this.Y = this.memoryManager.ReadValue<float>(new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.COORDS_Y });
            return this.Y;
        }
        public float GetZ()
        {
            this.Z = this.memoryManager.ReadValue<float>(new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.COORDS_Z });
            return this.Z;
        }
        public bool IsFound()
        {
            this.isFound = this.memoryManager.ReadValue<int>(new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, 0x0 }) == 19239528;
            //if (isFound)
            //    UpdateValues();
            return isFound;
        }
        public float GetFaceAngle()
        {
            this.faceAngle = this.memoryManager.ReadValue<float>(new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.FACEANGLE });
            return this.faceAngle;
        }
        public void SetFaceAngle(float angle)
        {
            this.memoryManager.WriteValue(angle, new int[] { Offsets.LOCALPLAYER_1, Offsets.LOCALPLAYER_2, Offsets.FACEANGLE });
        }
    }
}
