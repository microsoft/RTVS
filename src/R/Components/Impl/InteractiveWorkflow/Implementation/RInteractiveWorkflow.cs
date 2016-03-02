// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;

namespace Microsoft.VisualStudio.R.Package.Repl {
    public sealed class RInteractiveWorkflow : IRInteractiveWorkflow {
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;
        private readonly Action _onDispose;
        private readonly RInteractiveWorkflowOperations _operations;

        private bool _replLostFocus;
        private bool _disposed;

        public IRHistory History { get; }
        public IRSession RSession { get; }

        public IRInteractiveWorkflowOperations Operations => _operations;

        public IInteractiveWindowVisualComponent ActiveWindow { get; private set; }

        public RInteractiveWorkflow(IRSessionProvider sessionProvider
            , IRHistoryProvider historyProvider
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , IRHostClientApp hostClientApp
            , ICoreShell coreShell
            , IRSettings settings
            , Action onDispose) {

            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
            _settings = settings;
            _coreShell = coreShell;
            _onDispose = onDispose;

            RSession = sessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, hostClientApp);
            History = historyProvider.CreateRHistory(this);
            _operations = new RInteractiveWorkflowOperations();

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

        public async Task<IInteractiveWindowVisualComponent> GetOrCreateVisualComponent(IInteractiveWindowComponentContainerFactory componentContainerFactory, int instanceId = 0) {
            // Right now only one instance of interactive window is allowed
            if (ActiveWindow != null) {
                throw new InvalidOperationException("Right now only one instance of interactive window is allowed");
            }

            _coreShell.AssertIsOnMainThread();

            var evaluator = RInstallationHelper.VerifyRIsInstalled(_coreShell, _settings.RBasePath)
                ? new RInteractiveEvaluator(RSession, History, _coreShell, _settings)
                : (IInteractiveEvaluator) new NullInteractiveEvaluator();

            ActiveWindow = componentContainerFactory.Create(instanceId, evaluator);
            var interactiveWindow = ActiveWindow.InteractiveWindow;
            interactiveWindow.TextView.Closed += (_, __) => evaluator.Dispose();
            _operations.InteractiveWindow = interactiveWindow;

            await interactiveWindow.InitializeAsync();
            ActiveWindow.Container.UpdateCommandStatus(true);
            return ActiveWindow;
        }

        public void Dispose() {
            if (_disposed) {
                return;
            }
            _disposed = true;

            _activeTextViewTracker.LastActiveTextViewChanged -= LastActiveTextViewChanged;
            RSession.Disconnected -= RSessionDisconnected;
            Operations.Dispose();
            _onDispose();
        }
    }
}