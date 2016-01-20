using System;
using System.ComponentModel.Composition;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export(typeof(IRInteractiveSessionProvider))]
    internal class RInteractiveSessionProvider : IRInteractiveSessionProvider {
        private readonly Lazy<IRInteractiveSession> _lazy;

        [ImportingConstructor]
        public RInteractiveSessionProvider(IRSessionProvider sessionProvider, IRHistoryProvider historyProvider, IActiveRInteractiveWindowTracker activeRInteractiveWindowTracker) {
            _lazy = new Lazy<IRInteractiveSession>(() => new RInteractiveSession(sessionProvider, historyProvider, activeRInteractiveWindowTracker, RToolsSettings.Current));
        }

        public IRInteractiveSession GetOrCreate() {
            return _lazy.Value;
        }
    }
}