using System;

namespace FluffyKVM
{
  public interface IManager
  {
    event EventHandler<MessageActivityEventArgs> MessageActivity;

    void Stop();
  }
}