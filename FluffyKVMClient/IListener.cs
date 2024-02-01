using System;

namespace FluffyKVMClient
{
  public interface IListener : IDisposable
  {
    event EventHandler<string> MessageReceived;
  }
}