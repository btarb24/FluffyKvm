using System;
using System.IO.Ports;
using System.Linq;
using System.Management;

namespace FluffyKVMServer
{
  public class SerialBroadcaster : IBroadcaster
  {
    SerialPort _serialPort;

    public SerialBroadcaster(string port, int baudRate)
    {
      _serialPort = new SerialPort(port, baudRate);
      _serialPort.Open();
    }

    public void Dispose()
    {
      _serialPort.Dispose();
    }

    public void SendMessageToListener(string msg)
    {
      try
      {
        if (!_serialPort.IsOpen)
          _serialPort.Open();

        _serialPort.Write(msg + '\n');
      }
      catch { }
    }

    private string FindProlificCOMPort()
    {
      using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
      {
        var comPorts = SerialPort.GetPortNames();
        var portDescriptions = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());

        foreach (var comPort in comPorts)
        {
          var description = portDescriptions.FirstOrDefault(d => d.Contains(comPort));
          if (description?.Contains("Prolific PL2303GT") == true)
            return comPort;
        }
      }

      throw new Exception("Port not found");
    }
  }
}