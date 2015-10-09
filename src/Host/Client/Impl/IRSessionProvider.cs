using System;

namespace Microsoft.R.Host.Client
{
    public interface IRSessionProvider : IDisposable
    {
        IRSession Create(int sessionId);
        IRSession Current { get; }

        /// <summary>
        /// event raised when current session changes
        /// </summary>
        event EventHandler CurrentSessionChanged;
    }
}