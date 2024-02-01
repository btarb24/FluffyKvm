using MouseKeyboardLibrary.WinAPI;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace MouseKeyboardLibrary
{

  /// <summary>
  /// Abstract base class for Mouse and Keyboard hooks
  /// </summary>
  public abstract class GlobalHook
  {
    protected const int WH_MOUSE_LL = 14;
    protected const int WH_KEYBOARD_LL = 13;
    protected const int WH_KEYBOARD = 2;

    protected int _hookType;
    protected int _handleToHook;
    protected bool _isRunning;
    protected User32.HookProc _hookCallback;
    
    public bool IsStarted => _isRunning;

    public GlobalHook()
    {
      Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
    }
    
    public void Start()
    {
      //.net 3.5
      //var module = Assembly.GetExecutingAssembly().GetModules()[0];
      //var modulePtr = Marshal.GetHINSTANCE(module);

      var modulePtr = Kernel32.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);

      if (_isRunning || _hookType == 0)
        return;

      // Make sure we keep a reference to this delegate!
      // If not, GC randomly collects it, and a NullReference exception is thrown
      _hookCallback = new User32.HookProc(HookCallbackProcedure);

      _handleToHook = User32.SetWindowsHookEx(_hookType, _hookCallback, modulePtr, 0);

      // Were we able to sucessfully start hook?
      if (_handleToHook != 0)
      {
        _isRunning = true;
      }
    }

    public void Stop()
    {
      if (!_isRunning)
        return;

      User32.UnhookWindowsHookEx(_handleToHook);
      _isRunning = false;
    }

    protected virtual int HookCallbackProcedure(int nCode, Int32 wParam, IntPtr lParam)
    {
      // This method must be overriden by each extending hook
      return 0;
    }

    protected void Application_ApplicationExit(object sender, EventArgs e)
    {
      if (_isRunning)
      {
        Stop();
      }
    }

    protected static bool IsKeyDown(short KeyCode)
    {
      short state = User32.GetKeyState(KeyCode);
      return ((state & 0x10000) == 0x10000);
    }
  }
}