using System.Runtime.InteropServices;

namespace MouseKeyboardLibrary.WinAPI
{
  [StructLayout(LayoutKind.Sequential)]
  public class MouseLLHookStruct
  {
    public POINT pt;
    public int mouseData;
    public int flags;
    public int time;
    public int dwExtraInfo;
  }
}