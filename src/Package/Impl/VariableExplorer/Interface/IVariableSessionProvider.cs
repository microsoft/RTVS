using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.VariableWindow {
    /// <summary>
    /// Implemented by services that can provide a set of active variable
    /// sessions. Extensions should export their service using MEF with this
    /// interface as the contract.
    /// </summary>
    public interface IVariableSessionProvider {
        /// <summary>
        /// Returns a sequence of available variable sessions.
        /// </summary>
        IEnumerable<IVariableSession> GetSessions();

        /// <summary>
        /// Raised when the available sessions have changed.
        /// </summary>
        event EventHandler SessionsChanged;
    }
}
