using FluffyKVM;
using System;
using System.Windows;

namespace FluffyKVMClient
{
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      var window = new MainWindow("Fluffy KVM Client");
      window.StartSerialRequested += Window_StartSerialRequested;
      window.StartNetworkRequested += Window_StartNetworkRequested;
      window.ShowDialog();

      window.Closed += (s, a) => Environment.Exit(0);
    }

    private void Window_StartNetworkRequested(object sender, StartNetworkEventArgs e)
    {
      var client = new ClientSideManager();
      client.StartNetwork(e.Protocol, e.DestinationIp, e.Port);
      e.Manager = client;
    }

    private void Window_StartSerialRequested(object sender, StartSerialEventArgs e)
    {
      var client = new ClientSideManager();
      client.StartSerial(e.ComPort, e.BaudRate);
      e.Manager = client;
    }
  }
}