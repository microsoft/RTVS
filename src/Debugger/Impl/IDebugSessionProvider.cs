using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Debugger {
    public interface IDebugSessionProvider {
        Task<DebugSession> GetDebugSessionAsync(IRSession session, CancellationToken cancellationToken = default(CancellationToken));
    }
}
