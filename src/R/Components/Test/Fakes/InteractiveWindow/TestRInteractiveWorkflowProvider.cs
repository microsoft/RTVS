using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;

namespace Microsoft.R.Components.Test.Fakes.InteractiveWindow {
    [Export(typeof(IRInteractiveWorkflowProvider))]
    public class TestRInteractiveWorkflowProvider : IRInteractiveWorkflowProvider {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRHistoryProvider _historyProvider;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;
        private readonly ICoreShell _shell;
        private readonly IRSettings _settings;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;

        private Lazy<IRInteractiveWorkflow> _instanceLazy;

        [ImportingConstructor]
        public TestRInteractiveWorkflowProvider(IRSessionProvider sessionProvider
            , IRHistoryProvider historyProvider
            , IInteractiveWindowComponentContainerFactory componentContainerFactory
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , ICoreShell shell
            , IRSettings settings) {

            _sessionProvider = sessionProvider;
            _historyProvider = historyProvider;
            _componentContainerFactory = componentContainerFactory;
            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
            _shell = shell;
            _settings = settings;
        }

        public IRInteractiveWorkflow GetOrCreate() {
            Interlocked.CompareExchange(ref _instanceLazy, new Lazy<IRInteractiveWorkflow>(CreateRInteractiveWorkflow), null);
            return _instanceLazy.Value;
        }
        
        public Task<IInteractiveWindowVisualComponent> CreateInteractiveWindowAsync(IRInteractiveWorkflow workflow, int instanceId = 0) {
            return workflow.GetOrCreateVisualComponent(_componentContainerFactory, instanceId);
        }

        private IRInteractiveWorkflow CreateRInteractiveWorkflow() {
            return new RInteractiveWorkflow(_sessionProvider, _historyProvider, _activeTextViewTracker, _debuggerModeTracker, null, _shell, _settings, DisposeInstance);
        }

        private void DisposeInstance() {
            _instanceLazy = null;
        }
    }
}
