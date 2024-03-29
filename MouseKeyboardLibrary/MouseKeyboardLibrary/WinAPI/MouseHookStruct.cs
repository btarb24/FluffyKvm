﻿using System.Runtime.InteropServices;

namespace MouseKeyboardLibrary.WinAPI
{
  [StructLayout(LayoutKind.Sequential)]
  public class MouseHookStruct
  {
    public POINT pt;
    public int hwnd;
    public int wHitTestCode;
    public int dwExtraInfo;
  }
}