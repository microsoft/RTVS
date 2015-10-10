using System;
using System.Collections.Generic;

namespace Microsoft.R.Host.Client {
    public interface IRSessionProvider : IDisposable {
        IRSession Create(int sessionId);

        IRSession Current { get; }

        IReadOnlyDictionary<int, IRSession> GetSessions();

        /// <summary>
        /// event raised when current session changes
        /// </summary>
        event EventHandler CurrentSessionChanged;
    }
}