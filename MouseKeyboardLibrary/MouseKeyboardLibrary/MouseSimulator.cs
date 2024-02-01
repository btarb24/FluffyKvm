using MouseKeyboardLibrary.WinAPI;
using System.Drawing;
using System.Windows.Forms;

namespace MouseKeyboardLibrary
{
  /// <summary>
  /// Operations that simulate mouse events
  /// </summary>
  public static class MouseSimulator
  {
    const int XBUTTON1 = 0x0001;
    const int XBUTTON2 = 0x0002;

    /// <summary>
    /// Gets or sets a structure that represents both X and Y mouse coordinates
    /// </summary>
    public static Point Position
    {
      get => new Point(Cursor.Position.X, Cursor.Position.Y);      
      set => Cursor.Position = value;
    }

    /// <summary>
    /// Gets or sets only the mouse's x coordinate
    /// </summary>
    public static int X
    {
      get => Cursor.Position.X;
      set => Cursor.Position = new Point(value, Y);
    }

    /// <summary>
    /// Gets or sets only the mouse's y coordinate
    /// </summary>
    public static int Y
    {
      get => Cursor.Position.Y;
      set => Cursor.Position = new Point(X, value);
    }

    /// <summary>
    /// Press a mouse button down
    /// </summary>
    /// <param name="button"></param>
    public static void MouseDown(MouseButtons button)
    {
      switch (button)
      {
        case MouseButtons.Left:
          User32.mouse_event(MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0);
          break;
        case MouseButtons.Middle:
          User32.mouse_event(MOUSEEVENTF.MIDDLEDOWN, 0, 0, 0, 0);
          break;
        case MouseButtons.Right:
          User32.mouse_event(MOUSEEVENTF.RIGHTDOWN, 0, 0, 0, 0);
          break;
        case MouseButtons.XButton1:
          User32.mouse_event(MOUSEEVENTF.XDOWN, 0, 0, XBUTTON1, 0);
          break;
        case MouseButtons.XButton2:
          User32.mouse_event(MOUSEEVENTF.XDOWN, 0, 0, XBUTTON2, 0);
          break;
      }
    }

    /// <summary>
    /// Let a mouse button up
    /// </summary>
    /// <param name="button"></param>
    public static void MouseUp(MouseButtons button)
    {
      switch (button)
      {
        case MouseButtons.Left:
          User32.mouse_event(MOUSEEVENTF.LEFTUP, 0, 0, 0, 0);
          break;
        case MouseButtons.Middle:
          User32.mouse_event(MOUSEEVENTF.MIDDLEUP, 0, 0, 0, 0);
          break;
        case MouseButtons.Right:
          User32.mouse_event(MOUSEEVENTF.RIGHTUP, 0, 0, 0, 0);
          break;
        case MouseButtons.XButton1:
          User32.mouse_event(MOUSEEVENTF.XUP, 0, 0, XBUTTON1, 0);
          break;
        case MouseButtons.XButton2:
          User32.mouse_event(MOUSEEVENTF.XUP, 0, 0, XBUTTON2, 0);
          break;
      }
    }

    /// <summary>
    /// Click a mouse button (down then up)
    /// </summary>
    /// <param name="button"></param>
    public static void Click(MouseButtons button)
    {
      MouseDown(button);
      MouseUp(button);
    }

    /// <summary>
    /// Double click a mouse button (down then up twice)
    /// </summary>
    /// <param name="button"></param>
    public static void DoubleClick(MouseButtons button)
    {
      Click(button);
      Click(button);
    }

    /// <summary>
    /// Roll the mouse wheel. Delta of 120 wheels up once normally, -120 wheels down once normally
    /// </summary>
    /// <param name="delta"></param>
    public static void MouseWheel(int delta)
    {
      User32.mouse_event(MOUSEEVENTF.WHEEL, 0, 0, delta, 0);
    }
  }
}