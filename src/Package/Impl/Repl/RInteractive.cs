using System.Collections.Concurrent;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Options.R;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal class RInteractive : IRInteractive {
        private readonly IRToolsSettings _settings;
        private readonly ConcurrentDictionary<int, IInteractiveEvaluator> _evaluators = new ConcurrentDictionary<int, IInteractiveEvaluator>();
        public IRHistory History { get; }
        public IRSession RSession { get; }

        public RInteractive(IRSessionProvider sessionProvider, IRHistoryProvider historyProvider, IRToolsSettings settings) {
            _settings = settings;
            RSession = sessionProvider.GetInteractiveWindowRSession();
            History = historyProvider.CreateRHistory(this);
        }

        public IInteractiveEvaluator GetOrCreateEvaluator(int instanceId) {
            return _evaluators.GetOrAdd(instanceId, i => SupportedRVersions.VerifyRIsInstalled() ? new RInteractiveEvaluator(RSession, History, _settings) : (IInteractiveEvaluator)new NullInteractiveEvaluator());
        }
    }
}