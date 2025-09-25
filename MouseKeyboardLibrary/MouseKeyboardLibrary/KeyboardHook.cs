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
    private const int KF_REPEAT = 0x4000; //repeat doesn't work on low level (WH_KEYBOARD_LL)
    private readonly object _sync = new object();
    private bool _reportingEnabled = true;
    private bool _suppressRepeatedModifierKeys = true;
    private string _priorKeyEventDetails;

    public KeyboardHook()
    {
      _hookType = WH_KEYBOARD_LL;
    }

    public VirtualKeyShort QuickToggleKey { get; set; } = VirtualKeyShort.RCONTROL;

    public void SetRepeatedModifierSuppression(bool suppress)
    {
      _suppressRepeatedModifierKeys = suppress;
    }

    protected override int HookCallbackProcedure(int nCode, int wParam, IntPtr lParam)
    {
      lock (_sync)
      {
        if (nCode != HC_ACTION)
          return User32.CallNextHookEx(_handleToHook, nCode, wParam, lParam);

        var keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
        var isKeyDown = wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN;
        var repeat = HiWord(lParam) & KF_REPEAT;

        if (_suppressRepeatedModifierKeys && IsModifierKeyOnRepeat(keyboardHookStruct.vkCode, wParam, lParam))
        {
          Console.WriteLine("skipped repeat modifier");
          return 1;
        }
        _priorKeyEventDetails = GetKeyEventDetails(keyboardHookStruct.vkCode, wParam, lParam);

        Console.WriteLine($"vcode{keyboardHookStruct.vkCode}  repeat{repeat}  flags{keyboardHookStruct.flags}  time{keyboardHookStruct.time}   scan{keyboardHookStruct.scanCode}   info{keyboardHookStruct.dwExtraInfo}   ncode{nCode}  lparam{lParam}  wparam{wParam}");

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

    private bool IsModifierKeyOnRepeat(VirtualKeyShort vkCode, int wParam, IntPtr lParam)
    {
      switch (vkCode)
      {
        case VirtualKeyShort.CONTROL:
        case VirtualKeyShort.LCONTROL:
        case VirtualKeyShort.MENU:
        case VirtualKeyShort.LMENU:
        case VirtualKeyShort.RMENU:
        case VirtualKeyShort.SHIFT:
        case VirtualKeyShort.LSHIFT:
        case VirtualKeyShort.RSHIFT:
        case VirtualKeyShort.LWIN:
          return GetKeyEventDetails(vkCode, wParam, lParam) == _priorKeyEventDetails;

        default:
          return false;
      }
    }

    private string GetKeyEventDetails(VirtualKeyShort vkCode, int wParam, IntPtr lParam)
    {
      return $"{vkCode}_{wParam}_{lParam}";
    }
  }
}