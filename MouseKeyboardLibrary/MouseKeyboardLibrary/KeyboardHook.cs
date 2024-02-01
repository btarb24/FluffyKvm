using MouseKeyboardLibrary.WinAPI;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MouseKeyboardLibrary
{
  public class KeyboardHook : GlobalHook
  {
    public event KeyEventHandler KeyDown;
    public event KeyEventHandler KeyUp;
    public event EventHandler ReportingPaused;
    public event EventHandler ReportingResumed;
    
    private const int WM_KEYDOWN = 0x100;
    private const int WM_KEYUP = 0x101;
    private const int WM_SYSKEYDOWN = 0x104;
    private const int WM_SYSKEYUP = 0x105;

    private const int HC_ACTION = 0;
    private const int KF_REPEAT = 0x4000;

    public KeyboardHook()
    {
      _hookType = WH_KEYBOARD_LL;
    }

    public VirtualKeyShort QuickToggleKey { get; set; } = VirtualKeyShort.RCONTROL;
    private bool _reportingEnabled = true;

    protected override int HookCallbackProcedure(int nCode, int wParam, IntPtr lParam)
    {
      if (nCode != HC_ACTION)
        return User32.CallNextHookEx(_handleToHook, nCode, wParam, lParam);

      var keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));      
      var isKeyDown = wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN;
      var repeat = HiWord(lParam) & KF_REPEAT;
      Console.WriteLine($"repeat{repeat}  flags{keyboardHookStruct.flags}  time{keyboardHookStruct.time}   scan{keyboardHookStruct.scanCode}   info{keyboardHookStruct.dwExtraInfo}   ncode{nCode}  lparam{lParam}  wparam{wParam}");

      if (keyboardHookStruct.vkCode == QuickToggleKey)
      {
        if (isKeyDown)
          ToggleReporting();

        return User32.CallNextHookEx(_handleToHook, nCode, wParam, lParam);
      }

      if (!_reportingEnabled)
        return User32.CallNextHookEx(_handleToHook, nCode, wParam, lParam);

      var e = new KeyEventArgs((Keys)keyboardHookStruct.vkCode);
      if (isKeyDown)
      {
        KeyDown?.Invoke(this, e);
      }
      else
      {
        KeyUp?.Invoke(this, e);
      }

      if (e.Handled)
        return 1;

      return User32.CallNextHookEx(_handleToHook, nCode, wParam, lParam);
    }

    private void ToggleReporting()
    {
      _reportingEnabled = !_reportingEnabled;

      if (_reportingEnabled)
        ReportingResumed?.Invoke(this, EventArgs.Empty);
      else
        ReportingPaused?.Invoke(this, EventArgs.Empty);
    }

    private static ulong HiWord(IntPtr ptr)
    {
      if (((ulong)ptr & 0x80000000) == 0x80000000)
        return ((ulong)ptr >> 16);
      else
        return ((ulong)ptr >> 16) & 0xffff;
    }
  }
}