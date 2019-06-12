using System;
using System.Linq;
using System.Diagnostics;
using TreeGlide.Managers;

namespace TreeGlide
{
    public class MemoryManager
    {
        private Process process;
        private VAMemory vam;

        public MemoryManager(string process)//, Int32 baseAddress)
        {
            this.vam = new VAMemory(process);
            this.process = Process.GetProcessesByName(process).FirstOrDefault();
            //var baseAddress = (IntPtr)vam.ReadInt32(GetAddressSigScan(@"\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x83\xc4\x00\x8b\xe5\x5d\xc3\xcc\xcc\xcc\xcc\xcc\xcc\x55\x8b\xec\x83\xec\x00\x8d\x45\x00\xb9\x00\x00\x00\x00\x50\x8d\x45\x00\x50\x6a\x00\xe8\x00\x00\x00\x00\x8d\x45\x00\xb9\x00\x00\x00\x00\x50\x50\xe8\x00\x00\x00\x00\x8a\x45\x00\x88\x45\x00\x8d\x45\x00\x50\x8d\x45\x00\xc7\x45\x00\x00\x00\x00\x00\x50\x68\x00\x00\x00\x00\x68\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x83\xc4\x00\x3d\x00\x00\x00\x00\x75\x00\x83\xe8\x00\x8b\x00\x68\x00\x00\x00\x00\xa3\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x83\xc4\x00\x8b\xe5\x5d\xc3\xcc\xcc\xcc\xcc\xcc\xcc\xb9\x00\x00\x00\x00\xe8\x00\x00\x00\x00\xb9\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xc7\x05\x00\x00\x00\x00\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x68\x00\x00\x00\x00\xa3\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x59\xc3\xcc\xcc\xcc\xcc\xcc\xcc\xcc\xb9",
            //                                "xx????????xx????????xx????????xx????????x????xx?xxxxxxxxxxxxxxx?xx?x????xxx?xx?x????xx?x????xxx????xx?xx?xx?xxx?xx?????xx????x????x????xx?x????x?xx?xxx????x????xx????????xx????????xx????????xx????????x????xx?xxxxxxxxxxx????x????x????xx????????xx????????x????x????x????x????xxxxxxxxxx")
            //                                + 0x2) + 0x540;
            //Console.WriteLine("Read Address: 0x" + this.baseAddress.ToString("X"));
            //Console.WriteLine("Value at address: " + vam.ReadInt32(this.baseAddress).ToString("X"));
        }
        public IntPtr GetAddressSigScan(string pattern, string mask, Int32 offset)
        {
            var sigScan = new SigScanSharp(this.process, new IntPtr(0x40000), 0xFFFFFF);
            return (IntPtr) vam.ReadInt32(sigScan.FindPattern(pattern, mask, ((int)this.process.MainModule.BaseAddress) - 0x40000) + offset);
        }
        public IntPtr GetPointer(IntPtr baseAddress, int[] offsetArr)
        {
            if (offsetArr.Length == 0)
                return  (IntPtr)vam.ReadInt32(baseAddress);

            IntPtr pointer = IntPtr.Add((IntPtr)vam.ReadInt32(baseAddress), offsetArr[0]);
            for (int i = 1; i < offsetArr.Length; i++)
                pointer = IntPtr.Add((IntPtr)vam.ReadInt32(pointer), offsetArr[i]);

            return pointer;
        }
        public T ReadValue<T>(IntPtr baseAddress, int[] offsetArr)
        {
            var pointer = GetPointer(baseAddress, offsetArr);
            if (typeof(T).Equals(typeof(int)))
            {
                return (T)Convert.ChangeType(vam.ReadInt32(pointer), typeof(T));
            }
            else if (typeof(T).Equals(typeof(double)))
            {
                return (T)Convert.ChangeType(vam.ReadDouble(pointer), typeof(T));
            }
            else if (typeof(T).Equals(typeof(float)))
            {
                return (T)Convert.ChangeType(vam.ReadFloat(pointer), typeof(T));
            }
            return default;
        }
        public void WriteValue<T>(IntPtr baseAddress, T value, int[] offsetArr)
        {
            var pointer = GetPointer(baseAddress, offsetArr);
            Type type = value.GetType();
            if (type.Equals(typeof(int)))
            {
                vam.WriteInt32(pointer, Convert.ToInt32(value));
            }
            else if (type.Equals(typeof(float)))
            {
                vam.WriteFloat(pointer, (float) Convert.ToDouble(value));
            }
            else if (type.Equals(typeof(double)))
            {
                vam.WriteDouble(pointer, Convert.ToDouble(value));
            }
        }
    }
}
