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
        private IntPtr baseAddress;
        MemoryManager memoryManager;

        #region Offsets
        private struct Offsets
        {            
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
            this.baseAddress = memoryManager.GetAddressSigScan(@"\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x83\xc4\x00\x8b\xe5\x5d\xc3\xcc\xcc\xcc\xcc\xcc\xcc\x55\x8b\xec\x83\xec\x00\x8d\x45\x00\xb9\x00\x00\x00\x00\x50\x8d\x45\x00\x50\x6a\x00\xe8\x00\x00\x00\x00\x8d\x45\x00\xb9\x00\x00\x00\x00\x50\x50\xe8\x00\x00\x00\x00\x8a\x45\x00\x88\x45\x00\x8d\x45\x00\x50\x8d\x45\x00\xc7\x45\x00\x00\x00\x00\x00\x50\x68\x00\x00\x00\x00\x68\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x83\xc4\x00\x3d\x00\x00\x00\x00\x75\x00\x83\xe8\x00\x8b\x00\x68\x00\x00\x00\x00\xa3\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x83\xc4\x00\x8b\xe5\x5d\xc3\xcc\xcc\xcc\xcc\xcc\xcc\xb9\x00\x00\x00\x00\xe8\x00\x00\x00\x00\xb9\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x68\x00\x00\x00\x00\xa3\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x59\xc3\xcc\xcc\xcc\xcc\xcc\xcc\xcc\xb9",
                                            "xx????????xx????????xx????????xx????????x????xx?xxxxxxxxxxxxxxx?xx?x????xxx?xx?x????xx?x????xxx????xx?xx?xx?xxx?xx?????xx????x????x????xx?x????x?xx?xxx????x????xx????????xx????????xx????????xx????????x????xx?xxxxxxxxxxx????x????x????xx????????xx????????x????x????x????x????xxxxxxxxxx"
                                            , 0x2) + 0x540; //(Value from sigscanning Client_tos.exe + 813F6) + 0x2 = 190FCDC; 190FCDC + 0x540 = our localplayer address 
        }

        public void UpdateValues()
        {
            this.health = GetHealth();
            this.X = GetX();
            this.Y = GetY();
            this.Z = GetZ();
            this.attackSpeed = this.memoryManager.ReadValue<float>(this.baseAddress, new int[] { Offsets.ATTACKSPEED });
            this.movementSpeed = this.memoryManager.ReadValue<float>(this.baseAddress, new int[] { Offsets.MOVESPEED });
        }

        public int GetHealth()
        {
            this.health = this.memoryManager.ReadValue<int>(this.baseAddress, new int[] { Offsets.HEALTH });
            return this.health;
        }

        public void Teleport(float x, float y, float z)
        {
            memoryManager.WriteValue(this.baseAddress, x, new int[] { Offsets.COORDS_X });
            memoryManager.WriteValue(this.baseAddress, y, new int[] { Offsets.COORDS_Y });
            memoryManager.WriteValue(this.baseAddress, z, new int[] { Offsets.COORDS_Z });
        }

        public float GetX()
        {
            this.X = this.memoryManager.ReadValue<float>(this.baseAddress, new int[] { Offsets.COORDS_X });
            return this.X;
        }
        public float GetY()
        {
            this.Y = this.memoryManager.ReadValue<float>(this.baseAddress, new int[] { Offsets.COORDS_Y });
            return this.Y;
        }
        public float GetZ()
        {
            this.Z = this.memoryManager.ReadValue<float>(this.baseAddress, new int[] { Offsets.COORDS_Z });
            return this.Z;
        }
        public bool IsFound()
        {
            this.isFound = this.memoryManager.ReadValue<int>(this.baseAddress, new int[] { 0x0 }) == 19239528;
            //if (isFound)
            //    UpdateValues();
            return isFound;
        }
        public float GetFaceAngle()
        {
            this.faceAngle = this.memoryManager.ReadValue<float>(this.baseAddress, new int[] {  Offsets.FACEANGLE });
            return this.faceAngle;
        }
        public void SetFaceAngle(float angle)
        {
            this.memoryManager.WriteValue(this.baseAddress, angle, new int[] { Offsets.FACEANGLE });
        }
    }
}
