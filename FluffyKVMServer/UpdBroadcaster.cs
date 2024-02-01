using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FluffyKVMServer
{
  public class UdpBroadcaster : IBroadcaster
  {
    private readonly Socket _socket;
    private readonly IPEndPoint _endpoint;

    public UdpBroadcaster(IPAddress destinationIp, int port)
    {
      _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

      _endpoint = new IPEndPoint(destinationIp, port);
    }

    public void SendMessageToListener(string msg)
    {
      var buffer = Encoding.ASCII.GetBytes(msg);
      _socket.SendTo(buffer, _endpoint);
    }

    public void Dispose()
    {
      _socket.Dispose();
    }
  }
}