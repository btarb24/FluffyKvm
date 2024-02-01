using System;
using System.Runtime.InteropServices;

namespace FluffyKVM
{
  public static class InputSimulator
  {
    private static bool InjectMouseInputAvailable;
    static InputSimulator()
    {
      InjectMouseInputAvailable = true;

      try
      {
        MoveMouse(0, 0);
      }
      catch (EntryPointNotFoundException)
      {
        InjectMouseInputAvailable = false;
      }
    }

    public static void MoveMouse(int x, int y)
    {
      NativeMethods.INPUT mouse_input = default(NativeMethods.INPUT);
      mouse_input.type = 0;
      mouse_input.mi.dx = x;
      mouse_input.mi.dy = y;
      mouse_input.mi.mouseData = 0;
      mouse_input.mi.dwFlags = (int)NativeMethods.MouseEventFlags.MOUSEEVENTF_MOVE;

      SendInputEx(mouse_input);
    }
    
    private static uint SendInputEx(NativeMethods.INPUT input)
    {
      uint num = 0u;
      if (Environment.Is64BitOperatingSystem)
      {
        NativeMethods.INPUT64 iNPUT = default(NativeMethods.INPUT64);
        iNPUT.type = input.type;

        if (input.type == 1)
        {
          iNPUT.ki.wVk = input.ki.wVk;
          iNPUT.ki.wScan = input.ki.wScan;
          iNPUT.ki.dwFlags = input.ki.dwFlags;
          iNPUT.ki.time = input.ki.time;
          iNPUT.ki.dwExtraInfo = input.ki.dwExtraInfo;
        }
        else
        {
          iNPUT.mi.dx = input.mi.dx;
          iNPUT.mi.dy = input.mi.dy;
          iNPUT.mi.dwFlags = input.mi.dwFlags;
          iNPUT.mi.mouseData = input.mi.mouseData;
          iNPUT.mi.time = input.mi.time;
          iNPUT.mi.dwExtraInfo = input.mi.dwExtraInfo;
        }

        if (input.type == 0 && ((uint)input.mi.dwFlags & (true ? 1u : 0u)) != 0 && InjectMouseInputAvailable)
        {
          return NativeMethods.InjectMouseInput(new NativeMethods.MOUSEINPUT[1] { iNPUT.mi }, 1);
        }

        NativeMethods.INPUT64[] pInputs = new NativeMethods.INPUT64[1] { iNPUT };
        return NativeMethods.SendInput64(1u, pInputs, Marshal.SizeOf((object)iNPUT));
      }

      if (input.type == 0 && ((uint)input.mi.dwFlags & (true ? 1u : 0u)) != 0 && InjectMouseInputAvailable)
      {
        return NativeMethods.InjectMouseInput(new NativeMethods.MOUSEINPUT[1] { input.mi }, 1);
      }

      NativeMethods.INPUT[] pInputs2 = new NativeMethods.INPUT[1] { input };
      return NativeMethods.SendInput(1u, pInputs2, Marshal.SizeOf((object)input));
    }

    public struct KEYBDDATA
    {
      internal int wVk;

      internal int dwFlags;
    }
  }
}