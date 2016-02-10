using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.Options.R;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal sealed class RInteractiveWorkflow : IRInteractiveWorkflow {
        private readonly IContentType _contentType;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;
        private readonly IRToolsSettings _settings;
        private readonly ICoreShell _coreShell;
        private readonly Action _onDispose;

        private bool _replLostFocus;

        public IRHistory History { get; }
        public IRSession RSession { get; }
        public IRInteractiveWorkflowOperations Operations { get; }
        public IInteractiveWindowVisualComponent ActiveWindow { get; private set; }

        public RInteractiveWorkflow(IRSessionProvider sessionProvider
            , IRHistoryProvider historyProvider
            , IContentType contentType
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , ICoreShell coreShell
            , IRToolsSettings settings
            , Action onDispose) {

            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
            _settings = settings;
            _coreShell = coreShell;
            _onDispose = onDispose;
            _contentType = contentType;

            RSession = sessionProvider.GetInteractiveWindowRSession();
            History = historyProvider.CreateRHistory(this);
            Operations = new RInteractiveWorkflowOperations();

            _activeTextViewTracker.LastActiveTextViewChanged += LastActiveTextViewChanged;
            RSession.Disconnected += RSessionDisconnected;
        }

        private void LastActiveTextViewChanged(object sender, ActiveTextViewChangedEventArgs e) {
            if (ActiveWindow == null) {
                return;
            }

            if (ActiveWindow.TextView.Equals(e.Old) && !ActiveWindow.TextView.Equals(e.New)) {
                _replLostFocus = true;
                _coreShell.DispatchOnUIThread(CheckPossibleBreakModeFocusChange);
            }

            if (ActiveWindow.TextView.Equals(e.New)) {
                _coreShell.DispatchOnUIThread(Operations.PositionCaretAtPrompt);
            }
        }

        private void CheckPossibleBreakModeFocusChange() {
            if (ActiveWindow == null || !_debuggerModeTracker.IsEnteredBreakMode || !_replLostFocus) {
                return;
            }

            // When debugger hits a breakpoint it typically activates the editor.
            // This is not desirable when focus was in the interactive window
            // i.e. user worked in the REPL and not in the editor. Pull 
            // the focus back here. 
            ActiveWindow.Container.Show(true);
            _replLostFocus = false;
        }

        private void RSessionDisconnected(object o, EventArgs eventArgs) {
            Operations.ClearPendingInputs();
        }

        public async Task<IInteractiveWindowVisualComponent> CreateInteractiveWindowAsync(IInteractiveWindowComponentFactory interactiveWindowComponentFactory, int instanceId = 0) {
            // Right now only one instance of interactive window is allowed
            if (ActiveWindow != null) {
                throw new InvalidOperationException("Right now only one instance of interactive window is allowed");
            }

            var evaluator = SupportedRVersions.VerifyRIsInstalled()
                ? new RInteractiveEvaluator(RSession, History, _settings)
                : (IInteractiveEvaluator) new NullInteractiveEvaluator();

            ActiveWindow = interactiveWindowComponentFactory.Create(instanceId, evaluator);
            var interactiveWindow = ActiveWindow.InteractiveWindow;
            interactiveWindow.SetLanguage(RGuidList.RLanguageServiceGuid, _contentType);
            interactiveWindow.TextView.Closed += (_, __) => evaluator.Dispose();

            await interactiveWindow.InitializeAsync();
            ActiveWindow.Container.UpdateCommandStatus(true);
            return ActiveWindow;
        }

        public void Dispose() {
            _activeTextViewTracker.LastActiveTextViewChanged -= LastActiveTextViewChanged;
            RSession.Disconnected -= RSessionDisconnected;
            Operations.Dispose();
            _onDispose();
        }
    }
}