using System.Runtime.InteropServices;

namespace MouseKeyboardLibrary.WinAPI
{
  [StructLayout(LayoutKind.Sequential)]
  public class KeyboardHookStruct
  {
    public VirtualKeyShort vkCode;
    public int scanCode;
    public int flags;
    public int time;
    public int dwExtraInfo;
  }
}