
using System.Windows.Forms;

namespace MouseKeyboardLibrary
{
  public class HandleableMouseEventArgs : MouseEventArgs
  {
    public bool Handled { get; set; }

    public HandleableMouseEventArgs(MouseEventArgs args)
      : this(args.Button, args.Clicks, args.X, args.Y, args.Delta)
    { }

    public HandleableMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
      : base(button, clicks, x, y, delta)
    { }
  }
}