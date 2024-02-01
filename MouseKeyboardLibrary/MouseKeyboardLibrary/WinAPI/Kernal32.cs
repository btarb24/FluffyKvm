using System;
using System.Runtime.InteropServices;

namespace MouseKeyboardLibrary.WinAPI
{
  public static class Kernel32
  {
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string name);
  }
}