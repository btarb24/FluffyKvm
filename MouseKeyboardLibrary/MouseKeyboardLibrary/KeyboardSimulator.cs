using MouseKeyboardLibrary.WinAPI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MouseKeyboardLibrary
{
  public static class KeyboardSimulator
  {
    private enum KeySendState
    {
      Down,
      Up,
      DownAndUp
    }

    public static void KeyDown(Keys key)
    {
      var scanCode = GetScanCode(key);
      var isExtendedKey = IsExtendedKey(key);
      SendKeys(scanCode, KeySendState.Down, isExtendedKey);
    }

    public static void KeyUp(Keys key)
    {
      var scanCode = GetScanCode(key);
      var isExtendedKey = IsExtendedKey(key);
      SendKeys(scanCode, KeySendState.Up, isExtendedKey);
    }

    public static void KeyPress(Keys key)
    {
      KeyDown(key);
      KeyUp(key);
    }

    public static void SetCapsLockAs(bool desiredState)
    {
      if (Control.IsKeyLocked(Keys.CapsLock) != desiredState)
        SendKeys(ScanCodeShort.CAPITAL, KeySendState.DownAndUp, false);
    }

    public static void SetNumLockAs(bool desiredState)
    {
      if (Control.IsKeyLocked(Keys.NumLock) != desiredState)
        SendKeys(ScanCodeShort.NUMLOCK, KeySendState.DownAndUp, false);
    }

    public static void SetScrollLockAs(bool desiredState)
    {
      if (Control.IsKeyLocked(Keys.Scroll) != desiredState)
        SendKeys(ScanCodeShort.SCROLL, KeySendState.DownAndUp, false);
    }

    private static void SendKeys(ScanCodeShort key, KeySendState state, bool isExtendedKey)
    {
      var inputs = new List<INPUT>();

      if (state == KeySendState.Down || state == KeySendState.DownAndUp)
        inputs.Add(BuildInput(key, false, isExtendedKey));

      if (state == KeySendState.Up || state == KeySendState.DownAndUp)
        inputs.Add(BuildInput(key, true, isExtendedKey));

      var inputsArr = inputs.ToArray();
      var count = (uint)inputsArr.Length;
      var size = Marshal.SizeOf(typeof(INPUT));
      User32.SendInput(count, inputsArr, size);

      //hack to try to stop occassional missed ctrlUp when copying things
      if (key == ScanCodeShort.LCONTROL && state == KeySendState.Up)
        User32.SendInput(count, inputsArr, size);
    }

    private static INPUT BuildInput(ScanCodeShort key, bool isKeyUp, bool isExtendedKey)
    {
      var input = new INPUT();
      input.type = INPUT_TYPE.INPUT_KEYBOARD;
      input.U.ki.time = 0;
      input.U.ki.wVk = 0; //ignore VirtualKeys since we are using scanCodes instead
      input.U.ki.dwExtraInfo = UIntPtr.Zero;
      input.U.ki.wScan = key;
      input.U.ki.dwFlags = KEYEVENTF.SCANCODE;
      if (isKeyUp)
        input.U.ki.dwFlags |= KEYEVENTF.KEYUP;
      if (isExtendedKey)
        input.U.ki.dwFlags |= KEYEVENTF.EXTENDEDKEY;

      return input;
    }

    private static bool IsExtendedKey(Keys key)
    {
      switch (key)
      {
        case Keys.LWin:
        case Keys.RWin:
        case Keys.Left:
        case Keys.Right:
        case Keys.Up:
        case Keys.Down:
        case Keys.Home:
        case Keys.End:
        case Keys.PageUp:
        case Keys.PageDown:
        case Keys.Delete:
        case Keys.Divide:
        case Keys.Multiply:
        case Keys.Add:
        case Keys.Subtract:
        case Keys.Enter:
          return true;
        default:
          return false;
      }
    }

    private static ScanCodeShort GetScanCode(Keys key)
    {
      var virtualKeyCode = 0u;
      // Alt, Shift, and Control need to be changed for API function to work with them
      switch (key)
      {
        case Keys.Alt:
          virtualKeyCode = 18;
          break;
        case Keys.Control:
          virtualKeyCode = 17;
          break;
        case Keys.Shift:
          virtualKeyCode = 16;
          break;
        default:
          virtualKeyCode = (uint)key;
          break;
      }

      var scanCode = User32.MapVirtualKey(virtualKeyCode, MapVirtualKeyType.MAPVK_VK_TO_VSC);
      return (ScanCodeShort)scanCode;
    }
  }
}