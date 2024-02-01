using System;

namespace FluffyKVMServer
{
  public interface IBroadcaster : IDisposable
  {
    void SendMessageToListener(string msg);
  }
}