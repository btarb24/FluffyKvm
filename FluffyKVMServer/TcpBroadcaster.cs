using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FluffyKVMServer
{
  public class TcpBroadcaster : IBroadcaster
  {
    private readonly TcpClient _tcpClient;

    public TcpBroadcaster(IPAddress destinationIp, int port)
    {
      _tcpClient = new TcpClient();
      _tcpClient.Connect(destinationIp, port);
    }

    public void SendMessageToListener(string msg)
    {
      var buffer = Encoding.ASCII.GetBytes(msg);

      using (var stream = _tcpClient.GetStream())
      {
        stream.Write(buffer, 0, buffer.Length);
      }
    }

    public void Dispose()
    {
      _tcpClient.Dispose();
    }
  }
}