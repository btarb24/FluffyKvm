using System;
using System.Runtime.InteropServices;

namespace MouseKeyboardLibrary.WinAPI
{
  public static class User32
  {
    public delegate int HookProc(int nCode, int wParam, IntPtr lParam);


    [DllImport("user32.dll")]
    public static extern void mouse_event(MOUSEEVENTF flags, int dX, int dY, int buttons, int extraInfo);

    [DllImport("user32.dll")]
    public static extern void keybd_event(byte key, byte scan, int flags, int extraInfo);

    [DllImport("user32.dll")]
    public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern int UnhookWindowsHookEx(int idHook);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern int ToAscii(VirtualKeyShort uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

    [DllImport("user32.dll")]
    public static extern int GetKeyboardState(byte[] pbKeyState);

    [DllImport("user32.dll")]
    public static extern int GET_XBUTTON_WPARAM(int mouseData);

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern short GetKeyState(int vKey);

    [DllImport("user32.dll")]
    public static extern uint MapVirtualKey(uint uCode, MapVirtualKeyType uMapType);
  }
}