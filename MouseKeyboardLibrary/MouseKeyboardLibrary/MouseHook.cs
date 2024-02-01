using System;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MouseKeyboardLibrary.WinAPI;

namespace MouseKeyboardLibrary
{
  public class MouseHook : GlobalHook
  {
    private enum MouseEventType
    {
      None,
      MouseDown,
      MouseUp,
      DoubleClick,
      MouseWheel,
      MouseMove
    }

    public event EventHandler<HandleableMouseEventArgs> MouseDown;
    public event EventHandler<HandleableMouseEventArgs> MouseUp;
    public event EventHandler<HandleableMouseEventArgs> MouseMove;
    public event EventHandler<HandleableMouseEventArgs> MouseWheel;

    public event EventHandler Click;
    public event EventHandler DoubleClick;

    private const int WM_MOUSEWHEEL = 0x020A;
    private const int WM_MOUSEMOVE = 0x200;
    private const int WM_LBUTTONDOWN = 0x201;
    private const int WM_RBUTTONDOWN = 0x204;
    private const int WM_MBUTTONDOWN = 0x207;
    
    private const int WM_LBUTTONUP = 0x202;
    private const int WM_RBUTTONUP = 0x205;
    private const int WM_MBUTTONUP = 0x208;
    
    private const int WM_LBUTTONDBLCLK = 0x203;
    private const int WM_RBUTTONDBLCLK = 0x206;
    private const int WM_MBUTTONDBLCLK = 0x209;
    
    private const int WM_XBUTTONDOWN = 0x020B;
    private const int WM_XBUTTONUP = 0x020C;
    private const int WM_XBUTTONDBLCLK = 0x020D;
    
    private const int WM_NCXBUTTONDOWN = 0x00AB;
    private const int WM_NCXBUTTONUP = 0x00AC;
    private const int WM_NCXBUTTONDBLCLK = 0x00AD;

    public MouseHook()
    {
      _hookType = WH_MOUSE_LL;
    }

    public bool Enabled { get; set; } = true;

    protected override int HookCallbackProcedure(int nCode, int wParam, IntPtr lParam)
    {
      if (!Enabled)
        return User32.CallNextHookEx(_handleToHook, nCode, wParam, lParam);

      if (nCode > -1 && (MouseDown != null || MouseUp != null || MouseMove != null))
      {
        var mouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));
        var button = GetButton(wParam, mouseHookStruct);
        var eventType = GetEventType(wParam);

        var e = new MouseEventArgs(
            button,
            (eventType == MouseEventType.DoubleClick ? 2 : 1),
            mouseHookStruct.pt.x,
            mouseHookStruct.pt.y,
            (eventType == MouseEventType.MouseWheel ? (short)((mouseHookStruct.mouseData >> 16) & 0xffff) : 0));

        // Prevent multiple Right Click events (this probably happens for popup menus)
        if (button == MouseButtons.Right && mouseHookStruct.flags != 0)
        {
          eventType = MouseEventType.None;
        }

        switch (eventType)
        {
          case MouseEventType.MouseDown:
            if (MouseDown != null)
            {
              var args = new HandleableMouseEventArgs(e);
              MouseDown.Invoke(this, args);
              if (args.Handled)
                return 1;
            }
            break;
          case MouseEventType.MouseUp:
            Click?.Invoke(this, new EventArgs());
            if (MouseUp != null)
            {
              var args = new HandleableMouseEventArgs(e);
              MouseUp.Invoke(this, args);
              if (args.Handled)
                return 1;
            }
            break;
          case MouseEventType.DoubleClick:
            DoubleClick?.Invoke(this, new EventArgs());
            break;
          case MouseEventType.MouseWheel:
            if (MouseWheel != null)
            {
              var args = new HandleableMouseEventArgs(e);
              MouseWheel.Invoke(this, args);
              if (args.Handled)
                return 1;
            }
            break;
          case MouseEventType.MouseMove:
            if (MouseMove != null)
            {
              var args = new HandleableMouseEventArgs(e);
              MouseMove.Invoke(this, args);
              if (args.Handled)
                return 1;
            }
            break;
          default:
            break;
        }
      }

      return User32.CallNextHookEx(_handleToHook, nCode, wParam, lParam);
    }

    private MouseButtons GetButton(Int32 wParam, MouseLLHookStruct mouseHookStruct)
    {
      switch (wParam)
      {
        case WM_LBUTTONDOWN:
        case WM_LBUTTONUP:
        case WM_LBUTTONDBLCLK:
          return MouseButtons.Left;
        case WM_RBUTTONDOWN:
        case WM_RBUTTONUP:
        case WM_RBUTTONDBLCLK:
          return MouseButtons.Right;
        case WM_MBUTTONDOWN:
        case WM_MBUTTONUP:
        case WM_MBUTTONDBLCLK:
          return MouseButtons.Middle;
        case WM_XBUTTONDOWN:
        case WM_XBUTTONUP:
        case WM_XBUTTONDBLCLK:
        case WM_NCXBUTTONDOWN:
        case WM_NCXBUTTONUP:
        case WM_NCXBUTTONDBLCLK:
          return GetXButton(mouseHookStruct.mouseData);
        default:
          return MouseButtons.None;
      }
    }

    private MouseEventType GetEventType(int wParam)
    {
      switch (wParam)
      {
        case WM_LBUTTONDOWN:
        case WM_RBUTTONDOWN:
        case WM_MBUTTONDOWN:
        case WM_XBUTTONDOWN:
        case WM_NCXBUTTONDOWN:
          return MouseEventType.MouseDown;
        case WM_LBUTTONUP:
        case WM_RBUTTONUP:
        case WM_MBUTTONUP:
        case WM_XBUTTONUP:
        case WM_NCXBUTTONUP:
          return MouseEventType.MouseUp;
        case WM_LBUTTONDBLCLK:
        case WM_RBUTTONDBLCLK:
        case WM_MBUTTONDBLCLK:
        case WM_XBUTTONDBLCLK:
        case WM_NCXBUTTONDBLCLK:
          return MouseEventType.DoubleClick;
        case WM_MOUSEWHEEL:
          return MouseEventType.MouseWheel;
        case WM_MOUSEMOVE:
          return MouseEventType.MouseMove;
        default:
          return MouseEventType.None;
      }
    }

    /// <summary>
    /// The arguments sent to MouseProc don't specify which x button 
    /// was pressed, so we need to explicitly determine which button is down
    /// </summary>
    /// <returns>XButton1, XButton2, or None</returns>
    private MouseButtons GetXButton(int mouseData)
    {
      var xButton = 0;
      if ((mouseData & 0x80000000) == 0x80000000)
        xButton = (mouseData >> 16);
      else
        xButton = (mouseData >> 16) & 0xffff;

      if (xButton == 1)
        return MouseButtons.XButton1;
      else if (xButton == 2)
        return MouseButtons.XButton2;

      return MouseButtons.None;

      /* Couldn't find the method
       * 
      var button = GET_XBUTTON_WPARAM(mouseData);
      if (button == VK_XBUTTON1)
        return MouseButtons.XButton1;

      if (button == VK_XBUTTON2)
        return MouseButtons.XButton2;

      return 0;*/

      /*only seems to work if you aren't eating the mouse click
       * 
      if (IsKeyDown(VK_XBUTTON1))
        return MouseButtons.XButton1;

      if (IsKeyDown(VK_XBUTTON2))
        return MouseButtons.XButton2;

      return MouseButtons.None;*/
    }
  }
}