using System;

namespace DiabNet.Sync
{
    public class SyncException : Exception
    {
        public SyncException(string message, Exception? inner)
            : base(message, inner)
        {
        }
    }
}
