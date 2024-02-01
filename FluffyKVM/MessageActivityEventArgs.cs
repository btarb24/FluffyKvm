using System;

namespace FluffyKVM
{
  public class MessageActivityEventArgs : EventArgs
  {
    public string Message { get; }
    public MessageType MessageType { get; }

    public MessageActivityEventArgs(MessageType messageType, string message)
    {
      Message = message;
      MessageType = messageType;
    }
  }
}