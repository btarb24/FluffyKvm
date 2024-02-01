using System;

namespace FluffyKVM
{
  [Flags]
  public enum LockKeyStates
  {
    None = 0,
    CapsLock = 1,
    NumLock = 2,
    ScrollLock = 4
  }
}