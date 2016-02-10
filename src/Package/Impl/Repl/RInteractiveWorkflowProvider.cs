using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export(typeof(IRInteractiveWorkflowProvider))]
    internal class RInteractiveWorkflowProvider : IRInteractiveWorkflowProvider {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRHistoryProvider _historyProvider;
        private readonly IInteractiveWindowComponentFactory _interactiveWindowComponentFactory;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;

        private Lazy<IRInteractiveWorkflow> _instanceLazy;

        [ImportingConstructor]
        public RInteractiveWorkflowProvider(IRSessionProvider sessionProvider
            , IRHistoryProvider historyProvider
            , IInteractiveWindowComponentFactory interactiveWindowComponentFactory
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , IContentTypeRegistryService contentTypeRegistryService) {

            _sessionProvider = sessionProvider;
            _historyProvider = historyProvider;
            _interactiveWindowComponentFactory = interactiveWindowComponentFactory;
            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
            _contentTypeRegistryService = contentTypeRegistryService;
        }

        public IRInteractiveWorkflow GetOrCreate() {
            var contentType = _contentTypeRegistryService.GetContentType(RContentTypeDefinition.ContentType);
            Interlocked.CompareExchange(ref _instanceLazy, 
                new Lazy<IRInteractiveWorkflow>(() => new RInteractiveWorkflow(_sessionProvider, _historyProvider, contentType, _activeTextViewTracker, _debuggerModeTracker, EditorShell.Current, RToolsSettings.Current, DisposeInstance)),
                null);
            return _instanceLazy.Value;
        }

        public Task<IInteractiveWindowVisualComponent> CreateInteractiveWindowAsync(IRInteractiveWorkflow workflow, int instanceId = 0) {
            return workflow.CreateInteractiveWindowAsync(_interactiveWindowComponentFactory, instanceId);
        }

        private void DisposeInstance() {
            _instanceLazy = null;
        }
    }
}