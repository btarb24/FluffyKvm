using System;

namespace FluffyKVM
{
  public class StartSerialEventArgs : EventArgs
  {
    public string ComPort { get; }
    public int BaudRate { get; }

    public IManager Manager { get; set; }

    public StartSerialEventArgs(string comPort, int baudRate)
    {
      ComPort = comPort;
      BaudRate = baudRate;
    }
  }
}