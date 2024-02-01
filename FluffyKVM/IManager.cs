using System;

namespace FluffyKVM
{
  public interface IManager
  {
    event EventHandler<string> MessageActivity;

    void Stop();
  }
}