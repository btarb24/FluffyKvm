using FluffyKVM;
using MouseKeyboardLibrary;
using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace FluffyKVMServer
{
  public class ServerSideManager : IManager
  {
    private readonly MouseHook _mouseHook = new MouseHook();
    private readonly KeyboardHook _keyboardHook = new KeyboardHook();
    private IBroadcaster _broadcaster;
    private Point? _lastPoint;

    public event EventHandler<MessageActivityEventArgs> MessageActivity;
    //give a bailout key to give control of server back in case of exceptional situation
    private const Keys _toggleControl = Keys.RControlKey;
    private readonly Keys[] _lockKeys = new[] { Keys.CapsLock, Keys.NumLock, Keys.Scroll };

    public ServerSideManager()
    {
      _keyboardHook.KeyDown += KeyboardHook_KeyDown;
      _keyboardHook.KeyUp += KeyboardHook_KeyUp;
      _keyboardHook.ReportingPaused += (s, a) => _mouseHook.Enabled = false;
      _keyboardHook.ReportingResumed += (s, a) => _mouseHook.Enabled = true;

      _mouseHook.MouseMove += MouseHook_MouseMove;
      _mouseHook.MouseDown += MouseHook_MouseDown;
      _mouseHook.MouseUp += MouseHook_MouseUp;
      _mouseHook.MouseWheel += MouseHook_MouseWheel;
    }

    public void StartSerial(string port, int baudRate)
    {
      if (_broadcaster != null)
        return;
      
      _broadcaster = new SerialBroadcaster(port, baudRate);

      Start();
    }

    public void StartNetwork(TransportProtocol protocol, IPAddress destinationIp, int port)
    {
      if (_broadcaster != null)
        return;

      switch (protocol)
      {
        case TransportProtocol.TCP:
          _broadcaster = new TcpBroadcaster(destinationIp, port);
          break;
        case TransportProtocol.PCap:
        case TransportProtocol.UDP:
          _broadcaster = new UdpBroadcaster(destinationIp, port);
          break;
        default:
          throw new NotSupportedException();
      }

      Start();
    }

    public void Stop()
    {
      _keyboardHook.Stop();
      _mouseHook.Stop();

      _broadcaster.Dispose();
      _broadcaster = null;


      _lastPoint = null;

      MessageActivity?.Invoke(this, new MessageActivityEventArgs(MessageType.General, "*STOP*"));
    }

    private void Start()
    {
      _keyboardHook.Start();
      _mouseHook.Start();

      MessageActivity?.Invoke(this, new MessageActivityEventArgs(MessageType.General, "*START*"));
      SyncKeyLocks();
    }

    private void KeyboardHook_KeyDown(object sender, KeyEventArgs e)
    {
      //Console.WriteLine($"KeyDown: {e.KeyData} shift: {e.Shift}  modifiers: {e.Modifiers}");
      
      if (_lockKeys.Contains(e.KeyCode))
        return;

      e.Handled = true;
      SendMessage(MessageType.KeyDown, $"{(int)e.KeyCode}");
    }

    private void KeyboardHook_KeyUp(object sender, KeyEventArgs e)
    {
      Console.WriteLine($"KeyUp: {e.KeyData}");

      if (_lockKeys.Contains(e.KeyCode))
      {
        SyncKeyLocks();
        return;
      }

      SendMessage(MessageType.KeyUp, $"{(int)e.KeyCode}");
      e.Handled = true;
    }

    private void MouseHook_MouseMove(object sender, HandleableMouseEventArgs e)
    {
      if (_lastPoint == null)
      {
        _lastPoint = new Point(e.X, e.Y);
        return;
      }

      var position = Cursor.Position;
      var x = e.X - position.X;
      var y = e.Y - position.Y;

      var didntMove = x == 0 && y == 0;

      if (!didntMove)
        SendMessage(MessageType.MouseMove, $"{x}_{y}"); //relative offsets

      e.Handled = true;
    }

    private void MouseHook_MouseDown(object sender, HandleableMouseEventArgs e)
    {
      SendMessage(MessageType.MouseDown, $"{(int)e.Button}");
      e.Handled = true;
    }

    private void MouseHook_MouseUp(object sender, HandleableMouseEventArgs e)
    {
      SendMessage(MessageType.MouseUp, $"{(int)e.Button}");
      e.Handled = true;
    }

    private void MouseHook_MouseWheel(object sender, HandleableMouseEventArgs e)
    {
      SendMessage(MessageType.MouseWheel, $"{e.Delta}");
      e.Handled = true;
    }

    private void SyncKeyLocks()
    {
      LockKeyStates states = LockKeyStates.None;
      
      if (System.Windows.Input.Keyboard.IsKeyToggled(System.Windows.Input.Key.CapsLock))
        states |= LockKeyStates.CapsLock;
      if (System.Windows.Input.Keyboard.IsKeyToggled(System.Windows.Input.Key.NumLock))
        states |= LockKeyStates.NumLock;
      if (System.Windows.Input.Keyboard.IsKeyToggled(System.Windows.Input.Key.Scroll))
        states |= LockKeyStates.ScrollLock;

      SendMessage(MessageType.LockKeySync, $"{(int)states}");
    }

    private void SendMessage(MessageType messageType, string message)
    {
      var msgToSend = $"{(int)messageType}_{message}";

      _broadcaster.SendMessageToListener(msgToSend);

      MessageActivity?.Invoke(this, new MessageActivityEventArgs(messageType, msgToSend));
    }
  }
}