using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluffyKVMClient
{
  public class UdpListener : IListener
  {
    private UdpClient _client;

    public event EventHandler<string> MessageReceived;

    public UdpListener(IPAddress originIp, int port)
    {
      _client = new UdpClient(port);
      var endpoint = new IPEndPoint(originIp, port);

      Task.Run(() =>
      {
        while (_client != null)
        {
          try
          {
            var bytes = _client.Receive(ref endpoint);
            var message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            MessageReceived?.Invoke(this, message);
          }
          catch { }
        }
      });
    }

    public void Dispose()
    {
      var client = _client;
      _client = null;
      client.Dispose();
    }
  }
}