using System.Runtime.InteropServices;

namespace MouseKeyboardLibrary.WinAPI
{
  [StructLayout(LayoutKind.Sequential)]
  public struct INPUT
  {
    internal INPUT_TYPE type;
    internal InputUnion U;
    internal static int Size
    {
      get { return Marshal.SizeOf(typeof(INPUT)); }
    }
  }
}