﻿using System;
using System.Runtime.InteropServices;

namespace MouseKeyboardLibrary.WinAPI
{
  [StructLayout(LayoutKind.Sequential)]
  internal struct KEYBDINPUT
  {
    internal VirtualKeyShort wVk;
    internal ScanCodeShort wScan;
    internal KEYEVENTF dwFlags;
    internal int time;
    internal UIntPtr dwExtraInfo;
  }
}