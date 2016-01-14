using System;
using System.ComponentModel.Composition;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.History;

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export(typeof(IRInteractiveProvider))]
    internal class RInteractiveProvider : IRInteractiveProvider {
        private readonly Lazy<IRInteractive> _lazy;

        [ImportingConstructor]
        public RInteractiveProvider(IRSessionProvider sessionProvider, IRHistoryProvider historyProvider) {
            _lazy = new Lazy<IRInteractive>(() => new RInteractive(sessionProvider, historyProvider, RToolsSettings.Current));
        }

        public IRInteractive GetOrCreate() {
            return _lazy.Value;
        }
    }
}