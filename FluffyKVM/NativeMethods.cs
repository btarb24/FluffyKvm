using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FluffyKVM
{
  public static class NativeMethods
  {
    [Flags]
    internal enum KEYEVENTF
    {
      KEYDOWN = 0,
      EXTENDEDKEY = 1,
      KEYUP = 2,
      UNICODE = 4,
      SCANCODE = 8
    }

    [DllImport("user32.dll")]
    public static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint InjectMouseInput(MOUSEINPUT[] pInputs, int nInputs);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll", EntryPoint = "SendInput", SetLastError = true)]
    internal static extern uint SendInput64(uint nInputs, INPUT64[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Explicit)]
    [SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable")]
    internal struct INPUT
    {
      [FieldOffset(0)]
      internal int type;

      [FieldOffset(4)]
      internal MOUSEINPUT mi;

      [FieldOffset(4)]
      internal KEYBDINPUT ki;
    }

    public struct MOUSEINPUT
    {
      internal int dx;

      internal int dy;

      internal int mouseData;

      internal int dwFlags;

      internal int time;

      internal IntPtr dwExtraInfo;
    }

    public struct KEYBDINPUT
    {
      internal short wVk;

      internal short wScan;

      internal int dwFlags;

      internal int time;

      internal IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct INPUT64
    {
      [FieldOffset(0)]
      internal int type;

      [FieldOffset(8)]
      internal MOUSEINPUT mi;

      [FieldOffset(8)]
      internal KEYBDINPUT ki;
    }

    [Flags]
    public enum MouseEventFlags
    {
      MOUSEEVENTF_MOVE = 0x1,
      MOUSEEVENTF_LEFTDOWN = 0x2,
      MOUSEEVENTF_LEFTUP = 0x4,
      MOUSEEVENTF_RIGHTDOWN = 0x8,
      MOUSEEVENTF_RIGHTUP = 0x10,
      MOUSEEVENTF_MIDDLEDOWN = 0x20,
      MOUSEEVENTF_MIDDLEUP = 0x40,
      MOUSEEVENTF_WHEEL = 0x800,
      MOUSEEVENTF_ABSOLUTE = 0x8000,
      MOUSEEVENTF_XDOWN = 0x0080,
      MOUSEEVENTF_XUP = 0x0100
    }
  }
}