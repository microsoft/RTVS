using System;

namespace Microsoft.R.Host.Client
{
    public interface IRSessionProvider : IDisposable
    {
        IRSession Create(int sessionId);
        IRSession Current { get; }
    }
}