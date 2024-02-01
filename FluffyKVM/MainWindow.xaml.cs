﻿using System;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Windows;

namespace FluffyKVM
{
  public partial class MainWindow : Window
  {
    private IManager _manager;
    private bool _showCount;
    private int _count;

    public event EventHandler<StartSerialEventArgs> StartSerialRequested;
    public event EventHandler<StartNetworkEventArgs> StartNetworkRequested;

    public MainWindow(string title)
    {
      InitializeComponent();

      Title = title;

      PopulatePorts();
      LoadConfig();

      this.Closing += Window_Closing;
    }

    private void PopulatePorts()
    {
      using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
      {
        var portnames = SerialPort.GetPortNames();
        var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());

        var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();

        foreach (string port in portList)
        {
          cmbSerialPort.Items.Add(port);

          if (port.Contains("Prolific PL2303GT"))
            cmbSerialPort.SelectedItem = port;
        }
      }
    }

    private void Start()
    {
      if (_manager != null)
      {
        MessageBox.Show("Already running", "Error");
        return;
      }
      
      var protocol = GetProtocol();

      if (_manager == null)
      {
        if (protocol == TransportProtocol.USB)
        {
          var port = (cmbSerialPort.SelectionBoxItem as string).Split(' ').FirstOrDefault();
          var baudRate = int.Parse(cmbBaudRate.SelectionBoxItem as string);
          var args = new StartSerialEventArgs(port, baudRate);

          StartSerialRequested?.Invoke(this, args);

          _manager = args.Manager;
        }
        else
        {
          var port = int.Parse(txtDestinationPort.Text);
          var ip = IPAddress.Parse(txtDestinationAddress.Text);
          var args = new StartNetworkEventArgs(protocol, ip, port);

          StartNetworkRequested?.Invoke(this, args);

          _manager = args.Manager;
        }

        if (_manager == null)
          throw new Exception("Manager creation failed");

        _manager.MessageActivity += Manager_MessageActivity;
      }

      pnlSetup.IsEnabled = false;
      btnStop.IsEnabled = true;

      SaveConfig();
    }

    private void Stop()
    {
      pnlSetup.IsEnabled = true;
      btnStop.IsEnabled = false;

      _manager?.Stop();
      _manager = null;
    }

    private void Manager_MessageActivity(object sender, string msg)
    {
      _count++;
      Dispatcher.Invoke(() =>
      {
        if (chkShowCount.IsChecked == true)
          lblCount.Text = _count.ToString();

        if (chkShowMessages.IsChecked != true)
          return;

        lstMessages.Items.Insert(0, msg);

        if (lstMessages.Items.Count > 100)
          lstMessages.Items.RemoveAt(100);
      });
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      Stop();
    }

    private void ChkShowMessages_Checked(object sender, RoutedEventArgs e)
    {
      lstMessages.Items.Clear();
      if (chkShowMessages.IsChecked == true)
      {
        lstMessages.Visibility = Visibility.Visible;
      }
      else
      {
        lstMessages.Visibility = Visibility.Collapsed;
      }
    }

    private void ChkShowCount_Checked(object sender, RoutedEventArgs e)
    {
      lstMessages.Items.Clear();
      if (chkShowCount.IsChecked == true)
      {
        _showCount = true;
        lblCount.Visibility = Visibility.Visible;
      }
      else
      {
        _showCount = true;
        lblCount.Visibility = Visibility.Collapsed;
      }
    }

    private void chkNetwork_Checked(object sender, RoutedEventArgs e)
    {
      pnlNetworkSetup.Visibility = Visibility.Visible;
      pnlSerialSetup.Visibility = Visibility.Collapsed;
    }

    private void chkSerial_Checked(object sender, RoutedEventArgs e)
    {
      if (pnlNetworkSetup == null)
        return;

      pnlNetworkSetup.Visibility = Visibility.Collapsed;
      pnlSerialSetup.Visibility = Visibility.Visible;
    }

    private void BtnStart_Click(object sender, RoutedEventArgs e)
    {
      Start();
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
      Stop();
    }

    private TransportProtocol GetProtocol()
    {
      if (radUsb.IsChecked == true)
        return TransportProtocol.USB;

      if (radTcp.IsChecked == true)
        return TransportProtocol.TCP;

      if (radUdp.IsChecked == true)
        return TransportProtocol.UDP;

      if (radPCap.IsChecked == true)
        return TransportProtocol.PCap;

      throw new Exception("mystery protocol?");
    }

    private void SaveConfig()
    {
      Properties.Settings.Default["protocol"] = GetProtocol().ToString();
      Properties.Settings.Default["showMessages"] = chkShowMessages.IsChecked == true;
      Properties.Settings.Default["showCount"] = chkShowCount.IsChecked == true;
      Properties.Settings.Default["serialPort"] = cmbSerialPort.SelectionBoxItem as string;
      Properties.Settings.Default["baudRate"] = cmbBaudRate.SelectionBoxItem as string;
      Properties.Settings.Default["destinationIp"] = txtDestinationAddress.Text;
      Properties.Settings.Default["destinationPort"] = txtDestinationPort.Text;
      Properties.Settings.Default.Save();
    }

    private void LoadConfig()
    {
      Properties.Settings.Default.Reload();

      var protocolStr = Properties.Settings.Default.protocol;
      if (!string.IsNullOrEmpty(protocolStr))
      {
        var protocol = Enum.Parse(typeof(TransportProtocol), protocolStr);
        switch (protocol)
        {
          case TransportProtocol.USB:
            radUsb.IsChecked = true;
            break;
          case TransportProtocol.TCP:
            radTcp.IsChecked = true;
            break;
          case TransportProtocol.UDP:
            radUdp.IsChecked = true;
            break;
          case TransportProtocol.PCap:
            radPCap.IsChecked = true;
            break;
        }
      }

      chkShowMessages.IsChecked = Properties.Settings.Default.showMessages;
      chkShowCount.IsChecked = Properties.Settings.Default.showCount;

      var serialPortStr = Properties.Settings.Default.serialPort;
      if (!string.IsNullOrEmpty(serialPortStr))
      {
        var comPort = serialPortStr.Split(' ').FirstOrDefault();

        foreach (var port in cmbSerialPort.Items)
        {
          if ((port as string).StartsWith(comPort))
            cmbSerialPort.SelectedItem = port;
        }
      }

      var baudRateStr = Properties.Settings.Default.baudRate;
      if (!string.IsNullOrEmpty(baudRateStr))
        cmbBaudRate.Text = baudRateStr;

      txtDestinationAddress.Text = Properties.Settings.Default.destinationIp;
      txtDestinationPort.Text = Properties.Settings.Default.destinationPort;
    }
  }
}