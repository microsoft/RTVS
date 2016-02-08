using System;
using System.Collections.Generic;

namespace Microsoft.R.Host.Client {
    public interface IRSessionProvider : IDisposable {
        IRSession GetOrCreate(Guid guid, IRHostClientApp hostClientApp);
        IEnumerable<IRSession> GetSessions();
    }
}