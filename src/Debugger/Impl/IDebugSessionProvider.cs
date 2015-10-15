using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Debugger {
    public interface IDebugSessionProvider {
        Task<DebugSession> GetDebugSessionAsync(IRSession session);
    }
}
