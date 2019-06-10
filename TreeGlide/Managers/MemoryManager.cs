using System;
using System.Diagnostics;
using System.Linq;

namespace TreeGlide
{
    public class MemoryManager
    {
        private Process process;
        private IntPtr baseAddress;
        public IntPtr pointer;
        private VAMemory vam;

        public MemoryManager(string process)//, Int32 baseAddress)
        {
            this.vam = new VAMemory(process);
            this.process = Process.GetProcessesByName(process).FirstOrDefault();
            //this.baseAddress = GetAddressSigScan(@"D0 2D 4A A7 78 DB 0F A2 01 01 01 00 01", "xx????x");
            this.baseAddress = this.process.MainModule.BaseAddress + 0x15102FC;
            Console.WriteLine("Address value: " + ReadValue<int>(new int[] { 0x8, 0x8c, 0x428})); //this works and gives us our health value

            Console.WriteLine("Read Address: 0x" + this.baseAddress.ToString("X"));
            //\xd0\x2d\x00\x00\x00\x00\x0f xx????x
        }
        private IntPtr GetAddressSigScan(string pattern, string mask)
        {
            var sigScan = new SigScanSharp(this.process, new IntPtr(0x40000), 0xFFFFFF);
            return sigScan.FindPattern(pattern, mask, 0);
        }
        private void GetPointer(int[] offsetArr)
        {
            if (offsetArr.Length == 0)
            {
                this.pointer =  (IntPtr)vam.ReadInt32(this.baseAddress);
                return;
            }
            this.pointer = IntPtr.Add((IntPtr)vam.ReadInt32(this.baseAddress), offsetArr[0]);
            for (int i = 1; i < offsetArr.Length; i++)
            {
                this.pointer = IntPtr.Add((IntPtr)vam.ReadInt32(this.pointer), offsetArr[i]);
            }
        }
        public T ReadValue<T>(int[] offsetArr)
        {
            GetPointer(offsetArr);
            if (typeof(T).Equals(typeof(int)))
            {
                return (T)Convert.ChangeType(vam.ReadInt32(this.pointer), typeof(T));
            }
            else if (typeof(T).Equals(typeof(double)))
            {
                return (T)Convert.ChangeType(vam.ReadDouble(this.pointer), typeof(T));
            }
            else if (typeof(T).Equals(typeof(float)))
            {
                return (T)Convert.ChangeType(vam.ReadFloat(this.pointer), typeof(T));
            }
            return default;
        }
        public void WriteValue<T>(T value, int[] offsetArr)
        {
            GetPointer(offsetArr);
            Type type = value.GetType();
            if (type.Equals(typeof(int)))
            {
                vam.WriteInt32(this.pointer, Convert.ToInt32(value));
            }
            else if (type.Equals(typeof(float)))
            {
                vam.WriteFloat(this.pointer, (float) Convert.ToDouble(value));
            }
            else if (type.Equals(typeof(double)))
            {
                vam.WriteDouble(this.pointer, Convert.ToDouble(value));
            }
        }
    }
}
