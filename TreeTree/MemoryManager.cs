using System;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

namespace TreeTree
{
    public class MemoryManager
    {
        private Process process;
        private IntPtr baseAddress;
        public IntPtr pointer;
        private VAMemory vam;

        public MemoryManager(string process, Int32 baseAddress)
        {
            this.vam = new VAMemory(process);
            this.process = Process.GetProcessesByName(process).FirstOrDefault();
            this.baseAddress = this.process.MainModule.BaseAddress + baseAddress;
        }
        private void GetPointer(int[] offsetArr)
        {
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
