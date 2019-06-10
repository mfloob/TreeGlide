using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

//
// sigScan C# Implementation - Written by atom0s [aka Wiccaan]
// Class Version: 2.0.0
//
// [ CHANGE LOG ] -------------------------------------------------------------------------
//
//      2.0.0
//          - Updated to no longer require unsafe or fixed code.
//          - Removed unneeded methods and code.
//
//      1.0.0
//          - First version written and release.
//
// [ CREDITS ] ----------------------------------------------------------------------------
//
// sigScan is based on the FindPattern code written by
// dom1n1k and Patrick at GameDeception.net
//
// Full credit to them for the purpose of this code. I, atom0s, simply
// take credit for converting it to C#.
//
// [ USAGE ] ------------------------------------------------------------------------------
//
// Examples:
//
//      SigScan _sigScan = new SigScan();
//      _sigScan.Process = someProc;
//      _sigScan.Address = new IntPtr(0x123456);
//      _sigScan.Size = 0x1000;
//      IntPtr pAddr = _sigScan.FindPattern(new byte[]{ 0xFF, 0xFF, 0xFF, 0xFF, 0x51, 0x55, 0xFC, 0x11 }, "xxxx?xx?", 12);
//
//      SigScan _sigScan = new SigScan(someProc, new IntPtr(0x123456), 0x1000);
//      IntPtr pAddr = _sigScan.FindPattern(new byte[]{ 0xFF, 0xFF, 0xFF, 0xFF, 0x51, 0x55, 0xFC, 0x11 }, "xxxx?xx?", 12);
//
// ----------------------------------------------------------------------------------------
namespace TreeGlide
{
    public class SigScanSharp
    {
        private const int PROCESS_QUERY_INFORMATION = 0x400;
        private const int PROCESS_VM_READ = 0x10;

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2_0;
            public IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            public IntPtr Reserved3;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING
        {
            public short Length;
            public short MaximumLength;
            public IntPtr Buffer;
        }

        // for 32-bit process in a 64-bit OS only
        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION_WOW64
        {
            public long Reserved1;
            public long PebBaseAddress;
            public long Reserved2_0;
            public long Reserved2_1;
            public long UniqueProcessId;
            public long Reserved3;
        }

        // for 32-bit process in a 64-bit OS only
        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING_WOW64
        {
            public short Length;
            public short MaximumLength;
            public long Buffer;
        }
        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr ProcessHandle, int ProcessInformationClass, ref PROCESS_BASIC_INFORMATION ProcessInformation, int ProcessInformationLength, IntPtr ReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref IntPtr lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref UNICODE_STRING lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [MarshalAs(UnmanagedType.LPWStr)] string lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        // for 32-bit process in a 64-bit OS only
        [DllImport("ntdll.dll")]
        private static extern int NtWow64QueryInformationProcess64(IntPtr ProcessHandle, int ProcessInformationClass, ref PROCESS_BASIC_INFORMATION_WOW64 ProcessInformation, int ProcessInformationLength, IntPtr ReturnLength);

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int NtWow64ReadVirtualMemory64(IntPtr hProcess, long lpBaseAddress, [Out()] byte[] lpBuffer, long dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("ntdll.dll")]
        private static extern int NtWow64ReadVirtualMemory64(IntPtr hProcess, long lpBaseAddress, ref UNICODE_STRING_WOW64 lpBuffer, long dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("ntdll.dll")]
        private static extern int NtWow64ReadVirtualMemory64(IntPtr hProcess, long lpBaseAddress, [MarshalAs(UnmanagedType.LPWStr)] string lpBuffer, long dwSize, IntPtr lpNumberOfBytesRead);
        /// <summary>
        /// ReadProcessMemory
        /// 
        ///     API import definition for ReadProcessMemory.
        /// </summary>
        /// <param name="hProcess">Handle to the process we want to read from.</param>
        /// <param name="lpBaseAddress">The base address to start reading from.</param>
        /// <param name="lpBuffer">The return buffer to write the read data to.</param>
        /// <param name="dwSize">The size of data we wish to read.</param>
        /// <param name="lpNumberOfBytesRead">The number of bytes successfully read.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out()] byte[] lpBuffer,
            int dwSize
            );

        /// <summary>
        /// m_vDumpedRegion
        /// 
        ///     The memory dumped from the external process.
        /// </summary>
        private byte[] m_vDumpedRegion;

        /// <summary>
        /// m_vProcess
        /// 
        ///     The process we want to read the memory of.
        /// </summary>
        private Process m_vProcess;

        /// <summary>
        /// m_vAddress
        /// 
        ///     The starting address we want to begin reading at.
        /// </summary>
        private IntPtr m_vAddress;

        /// <summary>
        /// m_vSize
        /// 
        ///     The number of bytes we wish to read from the process.
        /// </summary>
        private Int32 m_vSize;


        #region "sigScan Class Construction"
        /// <summary>
        /// SigScan
        /// 
        ///     Main class constructor that uses no params. 
        ///     Simply initializes the class properties and 
        ///     expects the user to set them later.
        /// </summary>
        public SigScanSharp()
        {
            this.m_vProcess = null;
            this.m_vAddress = IntPtr.Zero;
            this.m_vSize = 0;
            this.m_vDumpedRegion = null;
        }
        /// <summary>
        /// SigScan
        /// 
        ///     Overloaded class constructor that sets the class
        ///     properties during construction.
        /// </summary>
        /// <param name="proc">The process to dump the memory from.</param>
        /// <param name="addr">The started address to begin the dump.</param>
        /// <param name="size">The size of the dump.</param>
        public SigScanSharp(Process proc, IntPtr addr, int size)
        {
            this.m_vProcess = proc;
            this.m_vAddress = addr;
            this.m_vSize = size;
        }
        #endregion

        #region "sigScan Class Private Methods"
        /// <summary>
        /// DumpMemory
        /// 
        ///     Internal memory dump function that uses the set class
        ///     properties to dump a memory region.
        /// </summary>
        /// <returns>Boolean based on RPM results and valid properties.</returns>
        private bool DumpMemory()
        {
            try
            {
                // Checks to ensure we have valid data.
                if (this.m_vProcess == null)
                    return false;
                if (this.m_vProcess.HasExited == true)
                    return false;
                if (this.m_vAddress == IntPtr.Zero)
                    return false;
                if (this.m_vSize == 0)
                    return false;

                // Create the region space to dump into.
                this.m_vDumpedRegion = new byte[this.m_vSize];

                bool bReturn = false;
                int nBytesRead = 0;

                // Dump the memory.
                //bReturn = ReadProcessMemory(
                //    this.m_vProcess.Handle, this.m_vAddress, this.m_vDumpedRegion, this.m_vSize
                //    );
                IntPtr handle = this.m_vProcess.Handle;
                if (handle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                int processParametersOffset = Environment.Is64BitOperatingSystem ? 0x20 : 0x10;
                try
                {
                    if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
                    {
                        //PROCESS_BASIC_INFORMATION_WOW64 pbi = new PROCESS_BASIC_INFORMATION_WOW64();
                        //int hr = NtWow64QueryInformationProcess64(handle, 0, ref pbi, Marshal.SizeOf(pbi), IntPtr.Zero);
                        //if (hr != 0)
                        //    throw new Win32Exception(hr);

                        //long pp = 0;
                        int result = NtWow64ReadVirtualMemory64(handle, (long)this.m_vProcess.MainModule.BaseAddress, this.m_vDumpedRegion, this.m_vSize, IntPtr.Zero);
                        if (result != 0)
                            throw new Win32Exception(result);

                        return result == 0;
                    }
                    //else // we are running with the same bitness as the OS, 32 or 64
                    //{
                    //    PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
                    //    int hr = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), IntPtr.Zero);
                    //    if (hr != 0)
                    //        throw new Win32Exception(hr);

                    //    IntPtr pp = new IntPtr();
                    //    if (!ReadProcessMemory(handle, pbi.PebBaseAddress + processParametersOffset, ref pp, new IntPtr(Marshal.SizeOf(pp)), IntPtr.Zero))
                    //        throw new Win32Exception(Marshal.GetLastWin32Error());

                    //    UNICODE_STRING us = new UNICODE_STRING();
                    //    if (!ReadProcessMemory(handle, pp, ref us, new IntPtr(Marshal.SizeOf(us)), IntPtr.Zero))
                    //        throw new Win32Exception(Marshal.GetLastWin32Error());

                    //    if ((us.Buffer == IntPtr.Zero) || (us.Length == 0))
                    //        return false;

                    //    string s = new string('\0', us.Length / 2);
                    //    if (!ReadProcessMemory(handle, us.Buffer, s, new IntPtr(us.Length), IntPtr.Zero))
                    //        throw new Win32Exception(Marshal.GetLastWin32Error());

                    //    return true;
                    //}
                }
                finally
                {
                    CloseHandle(handle);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        private bool MaskCheck(int nOffset, byte[] btPattern)
        {
            for (int x = 0; x < btPattern.Length; x++)
            {
                if (btPattern[x] == WILDCARD_CHAR)
                    continue;

                if ((btPattern[x] != this.m_vDumpedRegion[nOffset + x]))
                    return false;
            }

            return true;
        }
        private bool MaskCheck(int nOffset, IEnumerable<byte> btPattern, string strMask)
        {
            // Loop the pattern and compare to the mask and dump.
            return !btPattern.Where((t, x) => strMask[x] != '?' && ((strMask[x] == 'x') && (t != this.m_vDumpedRegion[nOffset + x]))).Any();

            // The loop was successful so we found the pattern.
        }
        #endregion

        #region "sigScan Class Public Methods"

        private const int WILDCARD_CHAR = 63; // the character '?';

        private static byte[] SigToByte(string Signature, byte wildcard)
        {
            Signature = Signature.Replace(" ", ""); // strip spaces
            byte[] pattern = new byte[(Signature.Length / 2)];
            int[] HexTable = new int[] { 0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,
                                   0x08,0x09,0x00,0x00,0x00,0x00,0x00,0x00,
                                   0x00,0x0A,0x0B,0x0C,0x0D,0x0E,0x0F };

            try
            {
                for (int x = 0, i = 0; i < Signature.Length; i += 2, x += 1)
                {
                    if (Signature[i] == wildcard)
                        pattern[x] = wildcard;                    
                    else
                        pattern[x] = (byte)(HexTable[Char.ToUpper(Signature[i]) - '0'] << 4 | HexTable[Char.ToUpper(Signature[i + 1]) - '0']);
                    Console.WriteLine(pattern[x]);
                }

                return pattern;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

        }

        private byte[] CodePatternToByteArr(string pattern)
        {
            var rawPattern = pattern.Substring(1, pattern.IndexOf("\\")).Replace("\\x", "") + pattern.Substring(pattern.IndexOf("\\"), pattern.Length).Replace("\\x", "");
            Console.WriteLine(rawPattern);
            var bytes = new byte[rawPattern.Length >> 1];
            for (int i = 0; i < rawPattern.Length >> 1; ++i)
            {
                bytes[i] = (byte)((GetHexVal(rawPattern[i << 1]) << 4) + (GetHexVal(rawPattern[(i << 1) + 1])));
            }
            return bytes;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public uint FindPattern(string pattern, int nOffset)
        {
            byte[] patternBytes = SigToByte(pattern, WILDCARD_CHAR);
            return (uint)FindPattern(patternBytes, nOffset);
        }

        public IntPtr FindPattern(string pattern, string mask, int nOffset)
        {
            var patternBytes = CodePatternToByteArr(pattern);
            return FindPattern(patternBytes, mask, nOffset);
        }

        public IntPtr FindPattern(byte[] btPattern, int nOffset)
        {
            try
            {
                // Dump the memory region if we have not dumped it yet.
                if (this.m_vDumpedRegion == null || this.m_vDumpedRegion.Length == 0)
                {
                    if (!this.DumpMemory())
                        return IntPtr.Zero;
                }

                // Loop the region and look for the pattern.
                for (int x = 0; x < this.m_vDumpedRegion.Length; x++)
                {
                    if (this.MaskCheck(x, btPattern))
                    {
                        // The pattern was found, return it.
                        return new IntPtr((int)this.m_vAddress + (x + nOffset));
                    }
                }

                // Pattern was not found.
                return IntPtr.Zero;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        public IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset)
        {
            try
            {
                // Dump the memory region if we have not dumped it yet.
                if (this.m_vDumpedRegion == null || this.m_vDumpedRegion.Length == 0)
                {
                    if (!this.DumpMemory())
                        return IntPtr.Zero;
                }

                // Ensure the mask and pattern lengths match.
                if (strMask.Length != btPattern.Length)
                {
                    Console.WriteLine("Mask does not match pattern.");
                    return IntPtr.Zero;
                }

                // Loop the region and look for the pattern.
                for (int x = 0; x < this.m_vDumpedRegion.Length; x++)
                {
                    if (this.MaskCheck(x, btPattern, strMask))
                    {
                        var result = new IntPtr((int)this.m_vAddress + (x + nOffset));
                        return new IntPtr((int)this.m_vAddress + (x + nOffset));
                    }
                }

                Console.WriteLine("Pattern not found.");
                return IntPtr.Zero;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// ResetRegion
        /// 
        ///     Resets the memory dump array to nothing to allow
        ///     the class to redump the memory.
        /// </summary>
        public void ResetRegion()
        {
            this.m_vDumpedRegion = null;
        }
        #endregion

        #region "sigScan Class Properties"
        public Process Process
        {
            get { return this.m_vProcess; }
            set { this.m_vProcess = value; }
        }
        public IntPtr Address
        {
            get { return this.m_vAddress; }
            set { this.m_vAddress = value; }
        }
        public Int32 Size
        {
            get { return this.m_vSize; }
            set { this.m_vSize = value; }
        }
        #endregion

    }
}