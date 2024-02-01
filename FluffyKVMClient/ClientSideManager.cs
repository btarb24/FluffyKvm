using FluffyKVM;
using MouseKeyboardLibrary;
using System;
using System.Net;
using System.Windows.Forms;

namespace FluffyKVMClient
{
  class ClientSideManager : IManager
  {
    private IListener _listener;
    
    public event EventHandler<MessageActivityEventArgs> MessageActivity;

    public ClientSideManager()
    {
      //     Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
    }

    public void StartSerial(string port, int baudRate)
    {
      if (_listener != null)
        return;

      _listener = new SerialListener(port, baudRate);

      _listener.MessageReceived += Listener_MessageReceived;

      MessageActivity?.Invoke(this, new MessageActivityEventArgs(MessageType.General, "*START*"));
    }

    public void StartNetwork(TransportProtocol protocol, IPAddress originIp, int port)
    {
      if (_listener != null)
        return;

      switch (protocol)
      {
        case TransportProtocol.TCP:
          _listener = new TcpListener(originIp, port);
          break;
        case TransportProtocol.UDP:
          _listener = new UdpListener(originIp, port);
          break;
        case TransportProtocol.PCap:
          _listener = new PCapListener(originIp, port);
          break;
        default:
          throw new NotSupportedException();
      }

      _listener.MessageReceived += Listener_MessageReceived;

      MessageActivity?.Invoke(this, new MessageActivityEventArgs(MessageType.General, "*START*"));
    }

    public void Stop()
    {
      _listener.MessageReceived -= Listener_MessageReceived;
      _listener.Dispose();
      _listener = null;
      MessageActivity?.Invoke(this, new MessageActivityEventArgs(MessageType.General, "*STOP*"));
    }

    private void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
    {
      Console.WriteLine(e.Reason);

      if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
      {
        //  InputDirector.ToLockScreen();
      }
      else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
      {
        InputDirector.ToUserSession();
      }
    }

    private void Listener_MessageReceived(object sender, string rawMessage)
    {
      var split = rawMessage.Split('_');
      var messageType = (MessageType)Enum.Parse(typeof(MessageType), split[0]);

      switch (messageType)
      {
        case MessageType.KeyUp:
          if (!int.TryParse(split[1], out var keyUp))
            return;

          KeyboardSimulator.KeyUp((Keys)keyUp);
          break;

        case MessageType.KeyDown:
          if (!int.TryParse(split[1], out var keyDown))
            return;

          KeyboardSimulator.KeyDown((Keys)keyDown);
          break;

        case MessageType.MouseMove:
          if (!int.TryParse(split[1], out var xOffset))
            return;
          if (!int.TryParse(split[2], out var yOffset))
            return;

          InputSimulator.MoveMouse(xOffset, yOffset);

          /* var position = Cursor.Position;
           position.X += xOffset;
           position.Y += yOffset;
           MouseSimulator.Position = position;*/
          break;

        case MessageType.MouseDown:
          if (!int.TryParse(split[1], out var mouseButtonDown))
            return;

          MouseSimulator.MouseDown((MouseButtons)mouseButtonDown);
          break;

        case MessageType.MouseUp:
          if (!int.TryParse(split[1], out var mouseButtonUp))
            return;

          MouseSimulator.MouseUp((MouseButtons)mouseButtonUp);
          break;

        case MessageType.MouseWheel:
          if (!int.TryParse(split[1], out var mouseWheelDelta))
            return;

          MouseSimulator.MouseWheel(mouseWheelDelta);
          break;

        case MessageType.LockKeySync:
          if (!int.TryParse(split[1], out var lockKeyStatesInt))
            return;

          var lockKeyStates = (LockKeyStates)lockKeyStatesInt;

          KeyboardSimulator.SetCapsLockAs(lockKeyStates.HasFlag(LockKeyStates.CapsLock));
          KeyboardSimulator.SetNumLockAs(lockKeyStates.HasFlag(LockKeyStates.NumLock));
          KeyboardSimulator.SetScrollLockAs(lockKeyStates.HasFlag(LockKeyStates.ScrollLock));
          break;

        default:
          break;
      }

      MessageActivity?.Invoke(this, new MessageActivityEventArgs(messageType, rawMessage));
    }
  }
}