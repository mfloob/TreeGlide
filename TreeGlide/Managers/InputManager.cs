using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

public class InputManager
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    public static void SetActiveWindow(String hWnd)
    {
        var prc = Process.GetProcessesByName(hWnd);
        if (prc.Length > 0)
        {
            SetForegroundWindow(prc[0].MainWindowHandle);
        }
    }
    public static Size GetWindowSize(String hWnd)
    {
        var prc = Process.GetProcessesByName(hWnd);
        return WindowScrape.Types.HwndObject.GetWindowByTitle(prc[0].MainWindowTitle).Size;
    }
    public static Point GetWindowPos(String hWnd)
    {
        var prc = Process.GetProcessesByName(hWnd);
        return WindowScrape.Types.HwndObject.GetWindowByTitle(prc[0].MainWindowTitle).Location;
    }

    //send key mouse input structure
    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public int mouseData;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    //send key keyboard inptu structure
    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public short wVk;
        public short wScan;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    public struct ScanCodes
    {
        internal const int ENTER = 0x1c;
        internal const int ESCAPE = 0x01;
        internal const int SPACE = 0x39;
        internal const int F = 0x21;
        internal const int B = 0x30;
        internal const int TWO = 0x03;
        internal const int ONE = 0x02;
        internal const int THREE = 0x04;
        internal const int FOUR = 0x05;
        internal const int FIVE = 0x06;
        internal const int SIX = 0x07;
        internal const int SEVEN = 0x08;
        internal const int EIGHT = 0x09;
        internal const int NINE = 0x0A;
        internal const int PERIOD = 0x34;
        internal const int Q = 0x10;
        internal const int W = 0x11;
        internal const int E = 0x12;
        internal const int R = 0x13;
        internal const int T = 0x14;
        internal const int Y = 0x15;
        internal const int U = 0x16;
        internal const int I = 0x17;
        internal const int O = 0x18;
        internal const int P = 0x19;
        internal const int A = 0x1E;
        internal const int S = 0x1F;
        internal const int D = 0x20;
        internal const int G = 0x22;
        internal const int H = 0x23;
        internal const int J = 0x24;
        internal const int K = 0x25;
        internal const int L = 0x26;
        internal const int Z = 0x2C;
        internal const int X = 0x2D;
        internal const int C = 0x2E;
        internal const int V = 0x2F;
        internal const int N = 0x31;
        internal const int M = 0x32;
        internal const int UP = 0x98;
        internal const int DOWN = 0x50;
        internal const int LEFT = 0x4B;
        internal const int RIGHT = 0x4D;
        internal const int NUMLOCK = 0x45;
    }

    //used to simulate dx key presses
    public static void CastKey(short key)
    {
        //send key data
        rawSend_Key(key, ActionFlags.KEYEVENTF_SCANCODE);
        Thread.Sleep(50);
        rawSend_Key(key, ActionFlags.KEYEVENTF_KEYUP | ActionFlags.KEYEVENTF_SCANCODE);
    }
    public static void CastKeyDown(short key)
    {
        //send key data
        rawSend_Key(key, ActionFlags.KEYEVENTF_SCANCODE);
    }
    public static void CastKeyUp(short key)
    {
        //send key data
        rawSend_Key(key, ActionFlags.KEYEVENTF_KEYUP | ActionFlags.KEYEVENTF_SCANCODE);
    }
    private static void rawSend_Key(short Keycode, int dwFlag)
    {
        INPUT[] InputData = new INPUT[1];
        InputData[0].type = 1; //keyboard
        InputData[0].ki.wScan = Keycode;
        InputData[0].ki.dwFlags = dwFlag;
        InputData[0].ki.time = 0;
        InputData[0].ki.dwExtraInfo = IntPtr.Zero;
        SendInput(1, InputData, Marshal.SizeOf(typeof(INPUT)));
    }

    //send key hardware type structure
    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public int uMsg;
        public short wParamL;
        public short wParamH;
    }

    //send key input structure
    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT
    {
        [FieldOffset(0)]
        public int type;
        [FieldOffset(4)]
        public MOUSEINPUT mi;
        [FieldOffset(4)]
        public KEYBDINPUT ki;
        [FieldOffset(4)]
        public HARDWAREINPUT hi;
    }
    //send key action flags
    internal struct ActionFlags
    {
        internal const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        internal const int KEYEVENTF_KEYUP = 0x0002;
        internal const int KEYEVENTF_UNICODE = 0x0004;
        internal const int KEYEVENTF_SCANCODE = 0x0008;
    }
    //detect key presses for hotkeys
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

    [DllImport("user32.dll")]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
    [Flags]
    public enum MouseEventFlags
    {
        LEFTDOWN = 0x00000002,
        LEFTUP = 0x00000004,
        MIDDLEDOWN = 0x00000020,
        MIDDLEUP = 0x00000040,
        MOVE = 0x00000001,
        ABSOLUTE = 0x00008000,
        RIGHTDOWN = 0x00000008,
        RIGHTUP = 0x00000010
    }

    public const Int32 CURSOR_SHOWING = 0x00000001;

    [StructLayout(LayoutKind.Sequential)]
    public struct CURSORINFO
    {
        public Int32 cbSize;        // Specifies the size, in bytes, of the structure. 
        public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
        public IntPtr hCursor;          // Handle to the cursor. 
        public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
    }
    public struct POINT
    {
        public Int32 X;
        public Int32 Y;
    }

    [DllImport("user32.dll", EntryPoint = "GetCursorInfo")]
    public static extern bool GetCursorInfo(out CURSORINFO pci);

    //simulates left mouse button
    public static void LeftMouseClick()
    {
        mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
        Thread.Sleep(50);
        mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        Thread.Sleep(100);
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetWindowDC(IntPtr window);
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern uint GetPixel(IntPtr dc, int x, int y);
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int ReleaseDC(IntPtr window, IntPtr dc);

    public static int GetColorAt(int x, int y)
    {
        IntPtr desk = GetDesktopWindow();
        IntPtr dc = GetWindowDC(desk);
        int a = (int)GetPixel(dc, x, y);
        ReleaseDC(desk, dc);
        Color col = Color.FromArgb(a);
        return col.ToArgb();
    }
}

