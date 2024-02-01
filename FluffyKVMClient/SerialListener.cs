using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace FluffyKVMClient
{
  public class SerialListener : IListener
  {
    private SerialPort _serialPort;
    private bool _disposed;

    public event EventHandler<string> MessageReceived;
    
    public SerialListener(string comPort, int baudRate)
    {
      _serialPort = new SerialPort(comPort, baudRate);
      _serialPort.Open();

      Task.Run(() =>
      {
        while (!_disposed)
        {
          try
          {
            Listen();
          }
          catch(Exception ex)
          { }
        }
      });
    }

    public void Dispose()
    {
      _disposed = true;
      _serialPort.Dispose();
    }

    private void Listen()
    {
      var message = _serialPort.ReadTo("\n");

      MessageReceived?.Invoke(this, message);
    }
  }
}