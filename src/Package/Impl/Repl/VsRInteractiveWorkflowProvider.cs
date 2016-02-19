using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export(typeof(IRInteractiveWorkflowProvider))]
    internal class VsRInteractiveWorkflowProvider : IRInteractiveWorkflowProvider {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRHistoryProvider _historyProvider;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;

        private Lazy<IRInteractiveWorkflow> _instanceLazy;

        [ImportingConstructor]
        public VsRInteractiveWorkflowProvider(IRSessionProvider sessionProvider
            , IRHistoryProvider historyProvider
            , IInteractiveWindowComponentContainerFactory componentContainerFactory
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker) {

            _sessionProvider = sessionProvider;
            _historyProvider = historyProvider;
            _componentContainerFactory = componentContainerFactory;
            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
        }

        public IRInteractiveWorkflow GetOrCreate() {
            Interlocked.CompareExchange(ref _instanceLazy, new Lazy<IRInteractiveWorkflow>(CreateRInteractiveWorkflow),null);
            return _instanceLazy.Value;
        }

        public Task<IInteractiveWindowVisualComponent> CreateInteractiveWindowAsync(IRInteractiveWorkflow workflow, int instanceId = 0) {
            return workflow.GetOrCreateVisualComponent(_componentContainerFactory, instanceId);
        }

        private IRInteractiveWorkflow CreateRInteractiveWorkflow() {
            var shell = EditorShell.Current;
            var settings = RToolsSettings.Current;
            return new RInteractiveWorkflow(_sessionProvider, _historyProvider, _activeTextViewTracker, _debuggerModeTracker, RHostClientApp.Instance, shell, settings, DisposeInstance);
        }

        private void DisposeInstance() {
            _instanceLazy = null;
        }
    }
}