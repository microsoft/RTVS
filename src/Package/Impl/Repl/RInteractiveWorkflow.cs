using System;
using System.Threading.Tasks;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.Options.R;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal sealed class RInteractiveWorkflow : IRInteractiveWorkflow {
        private readonly IRToolsSettings _settings;
        private readonly Action _onDispose;

        public IRHistory History { get; }
        public IRSession RSession { get; }
        public IRInteractiveWorkflowOperations Operations { get; }
        public IInteractiveWindowVisualComponent ActiveWindow { get; private set; }

        public RInteractiveWorkflow(IRSessionProvider sessionProvider, IRHistoryProvider historyProvider, IRToolsSettings settings, Action onDispose) {
            _settings = settings;
            _onDispose = onDispose;

            RSession = sessionProvider.GetInteractiveWindowRSession();
            History = historyProvider.CreateRHistory(this);
            Operations = new RInteractiveWorkflowOperations();

            RSession.Disconnected += RSessionDisconnected;
        }

        private void RSessionDisconnected(object o, EventArgs eventArgs) {
            Operations.ClearPendingInputs();
        }

        public async Task<IInteractiveWindowVisualComponent> CreateInteractiveWindowAsync(IInteractiveWindowComponentFactory interactiveWindowComponentFactory, IContentTypeRegistryService contentTypeRegistryService, int instanceId = 0) {
            // Right now only one instance of interactive window is allowed
            if (ActiveWindow != null) {
                throw new InvalidOperationException("Right now only one instance of interactive window is allowed");
            }

            var evaluator = SupportedRVersions.VerifyRIsInstalled()
                ? new RInteractiveEvaluator(RSession, History, _settings)
                : (IInteractiveEvaluator) new NullInteractiveEvaluator();

            ActiveWindow = interactiveWindowComponentFactory.Create(instanceId, evaluator);
            var interactiveWindow = ActiveWindow.InteractiveWindow;
            interactiveWindow.SetLanguage(RGuidList.RLanguageServiceGuid, contentTypeRegistryService.GetContentType(RContentTypeDefinition.ContentType));
            interactiveWindow.TextView.Closed += (_, __) => evaluator.Dispose();

            await interactiveWindow.InitializeAsync();
            ActiveWindow.Container.UpdateCommandStatus(true);
            return ActiveWindow;
        }

        public void Dispose() {
            RSession.Disconnected -= RSessionDisconnected;
            Operations.Dispose();
            _onDispose();
        }
    }
}