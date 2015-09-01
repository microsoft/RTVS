using System;
using System.Collections.Generic;

namespace Microsoft.R.Host.Client
{
    public class RResponseEventArgs : EventArgs
    {
        public IReadOnlyCollection<IRContext> Contexts { get; }
        public string Message { get; }

        public RResponseEventArgs(IReadOnlyCollection<IRContext> contexts, string message)
        {
            Contexts = contexts;
            Message = message;
        }
    }
}