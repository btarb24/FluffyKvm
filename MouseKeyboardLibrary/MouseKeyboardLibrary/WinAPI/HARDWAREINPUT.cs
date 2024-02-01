using System.Runtime.InteropServices;

namespace MouseKeyboardLibrary.WinAPI
{
  [StructLayout(LayoutKind.Sequential)]
  internal struct HARDWAREINPUT
  {
    internal int uMsg;
    internal short wParamL;
    internal short wParamH;
  }
}