using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpSocketListener = System.Net.Sockets.TcpListener;

namespace FluffyKVMClient
{
  public class TcpListener : IListener
  {
    private TcpSocketListener _listener;

    public event EventHandler<string> MessageReceived;

    public TcpListener(IPAddress hostAddress, int port)
    {
      _listener = new TcpSocketListener(hostAddress, port);
      Task.Run(() =>
      {
        _listener.Start();
        while (_listener != null)
        {
          try
          {
            var client = _listener.AcceptTcpClient();
            var stream = client.GetStream(); byte[] buffer = new byte[client.ReceiveBufferSize];

            int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
            var message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            client.Close();

            MessageReceived?.Invoke(this, message);
          }
          catch { }
        }
      });
    }

    public void Dispose()
    {
      var listener = _listener;
      _listener = null;
      listener.Stop();
    }
  }
}
