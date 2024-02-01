using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MouseKeyboardLibrary.WinAPI;

namespace MouseKeyboardLibrary
{
  public static class InputDirector
  {
    public static void ToLockScreen()
    {
      var list = new List<string>();
      GCHandle gch = GCHandle.Alloc(list);
      var callback = new User32.EnumWindowStationsDelegate(EnumWindowStationsCallback);

      User32.EnumWindowStations(callback, GCHandle.ToIntPtr(gch));

      Console.WriteLine("USER STATIONS:");
      foreach (string a in list)
      {
        Console.WriteLine(a);
        var desktops = GetDesktops(new IntPtr(Convert.ToInt32(a)));
        foreach (var b in desktops)
          Console.WriteLine($"   {b}");
      }
    }

    public static void ToUserSession()
    {
      var myDesktop = GetMyDesktop();
      var activeDesktop = GetInputDesktop();
      if (myDesktop.Equals(activeDesktop, StringComparison.CurrentCultureIgnoreCase)) ;


      /*  var station = User32.GetProcessWindowStation();
        Console.WriteLine($"user station: {station}");

        var desktops = GetDesktops(station);

        Console.WriteLine("USER DESKTOPS:");
        foreach (string desktop in desktops)
        {
          if (desktop == "Default")
          {

          }
        }*/
    }

    private static IList<string> GetDesktops(IntPtr station)
    {
      var list = new List<string>();
      GCHandle gch = GCHandle.Alloc(list);
      var callback = new User32.EnumDesktopsDelegate(EnumDesktopCallback);
      var desktops = User32.EnumDesktops(station, callback, GCHandle.ToIntPtr(gch));
      return list;
    }

    private static bool EnumWindowStationsCallback(string windowStation, IntPtr lParam)
    {
      GCHandle gch = GCHandle.FromIntPtr(lParam);
      IList<string> list = gch.Target as List<string>;

      if (null == list)
        return (false);

      list.Add(windowStation);

      return (true);
    }

    private static bool EnumDesktopCallback(string desktop, IntPtr lParam)
    {
      GCHandle gch = GCHandle.FromIntPtr(lParam);
      IList<string> list = gch.Target as List<string>;

      if (null == list)
        return (false);

      list.Add(desktop);

      return (true);
    }

    private static string GetMyDesktop()
    {
      var array = new byte[256];
      var threadDesktop = User32.GetThreadDesktop(Kernel32.GetCurrentThreadId());
      if (threadDesktop != IntPtr.Zero)
      {
        User32.GetUserObjectInformation(threadDesktop, 2, array, array.Length, out var _);
        return Encoding.ASCII.GetString(array).Replace("\0", "");
      }

      return "";
    }

    private static string GetInputDesktop()
    {
      var array = new byte[256];
      var intPtr = User32.OpenInputDesktop(0u, false, 1u);
      if (intPtr != IntPtr.Zero)
      {
        User32.GetUserObjectInformation(intPtr, 2, array, array.Length, out var _);
        return Encoding.ASCII.GetString(array).Replace("\0", "");
      }

      return "";
    }
  }
}