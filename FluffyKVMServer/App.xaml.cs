using FluffyKVM;
using System;
using System.Windows;

namespace FluffyKVMServer
{
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      var window = new MainWindow(LaunchMode.Server);
      window.StartSerialRequested += Window_StartSerialRequested;
      window.StartNetworkRequested += Window_StartNetworkRequested;
      window.ShowDialog();

      window.Closed += (s, a) => Environment.Exit(0);
    }

    private void Window_StartNetworkRequested(object sender, StartNetworkEventArgs e)
    {
      var server = new ServerSideManager();
      server.StartNetwork(e.Protocol, e.DestinationIp, e.Port);
      e.Manager = server;
    }

    private void Window_StartSerialRequested(object sender, StartSerialEventArgs e)
    {
      var server = new ServerSideManager();
      server.StartSerial(e.ComPort, e.BaudRate);
      e.Manager = server;
    }
  }
}