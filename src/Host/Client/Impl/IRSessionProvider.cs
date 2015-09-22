using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.R.Host.Client
{
    public interface IRSessionProvider : IDisposable
    {
        IRSession Create(int sessionId);
        IRSession Current { get; }
    }
}