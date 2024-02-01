using SharpPcap;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace FluffyKVMClient
{
  /* This is used because ZScaler is blocking UDP Packets.  However, they can still be captured when
   * using PCap.. which is a protocol-level library that captures all incoming packets (it's what zscaler 
   * uses too).  We can then filter out our specific packets via protocol & port.
   * 
   * We still need the UdpClient to open up the UDP port
   */
  public class PCapListener : IListener
  {
    private UdpListener _udpListener;
    private ILiveDevice _networkDevice;

    public event EventHandler<string> MessageReceived;

    public PCapListener(IPAddress originIp, int port)
    {
      _udpListener = new UdpListener(originIp, port);

      var ipSplit = originIp.ToString().Split('.');
      var baseIp = $"{ipSplit[0]}.{ipSplit[1]}.{ipSplit[2]}.";

      // Retrieve the device list from the local machine
      var allDevices = CaptureDeviceList.Instance;
      _networkDevice = allDevices.FirstOrDefault(d => d.ToString().Contains(baseIp));

      _networkDevice.Open(DeviceModes.MaxResponsiveness, -1);
      _networkDevice.Filter = $"udp port {port}";
      _networkDevice.OnPacketArrival += NetworkDevice_OnPacketArrival;
      _networkDevice.StartCapture();
    }

    private void NetworkDevice_OnPacketArrival(object sender, PacketCapture e)
    {
      // var rawPacket = e.GetPacket();
      // var bla = PacketDotNet.Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
      //var udp = bla.Extract<PacketDotNet.UdpPacket>();

      //var message = Encoding.ASCII.GetString(udp.PayloadData, 0, udp.PayloadData.Length);

      MessageReceived?.Invoke(this, "2_-1_-1");
    }

    public void Dispose()
    {
      _udpListener?.Dispose();
      _udpListener = null;

      _networkDevice?.Close();
      _networkDevice?.Dispose();
      _networkDevice = null;
    }
  }
}
