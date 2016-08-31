// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Workspace;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.History;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.VisualStudio.R.Package.Repl;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public sealed class RInteractiveWorkflow : IRInteractiveWorkflow {
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;
        private readonly IRSettings _settings;
        private readonly Action _onDispose;
        private readonly RInteractiveWorkflowOperations _operations;
        private readonly IWorkspaceServices _wss;

        private bool _replLostFocus;
        private bool _disposed;
        private bool _debuggerJustEnteredBreakMode;

        public ICoreShell Shell { get; }
        public IRHostBrokerConnector BrokerConnector { get; }
        public IConnectionManager Connections { get; }
        public IRHistory History { get; }
        public IRSession RSession { get; }
        public IRPackageManager Packages { get; }
        public IRPlotManager Plots { get; }

        public IRInteractiveWorkflowOperations Operations => _operations;

        public IInteractiveWindowVisualComponent ActiveWindow { get; private set; }

        public RInteractiveWorkflow(IRSessionProvider sessionProvider
            , IConnectionManagerProvider connectionsProvider
            , IRHistoryProvider historyProvider
            , IRPackageManagerProvider packagesProvider
            , IRPlotManagerProvider plotsProvider
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , IRHostBrokerConnector brokerConnector
            , ICoreShell coreShell
            , IRSettings settings
            , IWorkspaceServices wss
            , Action onDispose) {

            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
            _settings = settings;
            _wss = wss;
            _onDispose = onDispose;

            Shell = coreShell;
            BrokerConnector = brokerConnector;

            RSession = sessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, brokerConnector);
            Connections = connectionsProvider.CreateConnectionManager(this);

            History = historyProvider.CreateRHistory(this);
            Packages = packagesProvider.CreateRPackageManager(sessionProvider, settings, this);
            Plots = plotsProvider.CreatePlotManager(settings, this);
            _operations = new RInteractiveWorkflowOperations(this, _debuggerModeTracker, Shell);

            _activeTextViewTracker.LastActiveTextViewChanged += LastActiveTextViewChanged;
            RSession.Disconnected += RSessionDisconnected;

            _debuggerModeTracker.EnterBreakMode += DebuggerEnteredBreakMode;
            _debuggerModeTracker.LeaveBreakMode += DebuggerLeftBreakMode;
        }

        private void DebuggerEnteredBreakMode(object sender, EventArgs e) {
            _debuggerJustEnteredBreakMode = true;
        }

        private void DebuggerLeftBreakMode(object sender, EventArgs e) {
            _debuggerJustEnteredBreakMode = false;
        }

        private void LastActiveTextViewChanged(object sender, ActiveTextViewChangedEventArgs e) {
            if (ActiveWindow == null) {
                return;
            }

            // Check if REPL lost focus and focus moved to the editor
            if (!ActiveWindow.TextView.HasAggregateFocus && !string.IsNullOrEmpty(e.New?.TextBuffer?.GetFilePath())) {
                _replLostFocus = true;
                Shell.DispatchOnUIThread(CheckPossibleBreakModeFocusChange);
            }

            if (ActiveWindow.TextView.HasAggregateFocus) {
                Shell.DispatchOnUIThread(Operations.PositionCaretAtPrompt);
            }
        }

        private void CheckPossibleBreakModeFocusChange() {

            if (ActiveWindow != null && _debuggerJustEnteredBreakMode && _replLostFocus) {
                // When debugger hits a breakpoint it typically activates the editor.
                // This is not desirable when focus was in the interactive window
                // i.e. user worked in the REPL and not in the editor. Pull 
                // the focus back here. 
                _replLostFocus = false;
                if (IsRProjectActive()) {
                    ActiveWindow.Container.Show(true);
                }

                // Reset the flag, so that further focus changes are not affected until the next debugger break occurs.
                _debuggerJustEnteredBreakMode = false;
            }
        }

        private void RSessionDisconnected(object o, EventArgs eventArgs) {
            Operations.ClearPendingInputs();
        }

        private bool IsRProjectActive() {
            var wss = Shell.ExportProvider.GetExportedValue<IWorkspaceServices>();
            return wss.IsRProjectActive;
        }

        public async Task<IInteractiveWindowVisualComponent> GetOrCreateVisualComponent(IInteractiveWindowComponentContainerFactory componentContainerFactory, int instanceId = 0) {
            Shell.AssertIsOnMainThread();

            if (ActiveWindow != null) {
                // Right now only one instance of interactive window is allowed
                if (instanceId != 0) {
                    throw new InvalidOperationException("Right now only one instance of interactive window is allowed");
                }

                return ActiveWindow;
            }

            var evaluator = new RInteractiveEvaluator(RSession, History, Connections, Shell, _settings);

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

            _debuggerModeTracker.EnterBreakMode -= DebuggerEnteredBreakMode;
            _debuggerModeTracker.LeaveBreakMode -= DebuggerLeftBreakMode;
            _activeTextViewTracker.LastActiveTextViewChanged -= LastActiveTextViewChanged;
            RSession.Disconnected -= RSessionDisconnected;
            Operations.Dispose();
            Connections.Dispose();
            _onDispose();
        }
    }
}