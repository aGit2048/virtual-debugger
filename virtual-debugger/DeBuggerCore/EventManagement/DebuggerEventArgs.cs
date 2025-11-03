using System;

namespace DeBuggerCore.EventManagement
{
    public class DebuggerEventArgs : EventArgs
    {
        public static int ID => nameof(DebuggerEventArgs).GetHashCode();

        public int GetId() => ID;
    }
}
