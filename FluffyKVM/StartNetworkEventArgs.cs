using System;
using System.Net;

namespace FluffyKVM
{
  public class StartNetworkEventArgs : EventArgs
  {
    public TransportProtocol Protocol { get; }
    public IPAddress DestinationIp { get; }
    public int Port { get; }

    public IManager Manager { get; set; }

    public StartNetworkEventArgs(TransportProtocol protocol, IPAddress destinationIp, int port)
    {
      Protocol = protocol;
      DestinationIp = destinationIp;
      Port = port;
    }
  }
}