using Microsoft.R.Host.Client;

namespace Microsoft.R.Debugger {
    public interface IDebugSessionProvider {
        DebugSession GetDebugSession(IRSession session);
    }
}
