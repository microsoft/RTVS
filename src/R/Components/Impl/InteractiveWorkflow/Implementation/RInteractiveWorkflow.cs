// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.History;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Settings.Mirrors;
using Microsoft.R.Components.Workspace;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Repl;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public sealed class RInteractiveWorkflow : IRInteractiveWorkflow {
        private readonly DisposableBag _disposableBag;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;
        private readonly IRSettings _settings;
        private readonly RInteractiveWorkflowOperations _operations;

        private bool _replLostFocus;
        private bool _debuggerJustEnteredBreakMode;

        public ICoreShell Shell { get; }
        public IConnectionManager Connections { get; }
        public IRHistory History { get; }
        public IRSessionProvider RSessions { get; }
        public IRSession RSession { get; }
        public IRPackageManager Packages { get; }
        public IRPlotManager Plots { get; }

        public IRInteractiveWorkflowOperations Operations => _operations;

        public IInteractiveWindowVisualComponent ActiveWindow { get; private set; }

        public RInteractiveWorkflow(IConnectionManagerProvider connectionsProvider
            , IRHistoryProvider historyProvider
            , IRPackageManagerProvider packagesProvider
            , IRPlotManagerProvider plotsProvider
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , ICoreShell coreShell
            , IRSettings settings) {

            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
            _settings = settings;

            Shell = coreShell;
            RSessions = new RSessionProvider(coreShell.Services, new InteractiveWindowConsole(this));
            RSessions.BrokerChanging += OnBrokerChanging;

            RSession = RSessions.GetOrCreate(GuidList.InteractiveWindowRSessionGuid);
            Connections = connectionsProvider.CreateConnectionManager(this);

            History = historyProvider.CreateRHistory(this);
            Packages = packagesProvider.CreateRPackageManager(settings, this);
            Plots = plotsProvider.CreatePlotManager(settings, this, new FileSystem());
            _operations = new RInteractiveWorkflowOperations(this, _debuggerModeTracker, Shell);

            _activeTextViewTracker.LastActiveTextViewChanged += LastActiveTextViewChanged;
            RSession.Disconnected += RSessionDisconnected;

            _debuggerModeTracker.EnterBreakMode += DebuggerEnteredBreakMode;
            _debuggerModeTracker.LeaveBreakMode += DebuggerLeftBreakMode;

            _settings.PropertyChanged += OnSettingsChanged;

            _disposableBag = DisposableBag.Create<RInteractiveWorkflow>()
                .Add(() => _settings.PropertyChanged -= OnSettingsChanged)
                .Add(() => _debuggerModeTracker.EnterBreakMode -= DebuggerEnteredBreakMode)
                .Add(() => _debuggerModeTracker.LeaveBreakMode -= DebuggerLeftBreakMode)
                .Add(() => _activeTextViewTracker.LastActiveTextViewChanged -= LastActiveTextViewChanged)
                .Add(() => RSessions.BrokerChanging -= OnBrokerChanging)
                .Add(() => RSession.Disconnected -= RSessionDisconnected)
                .Add(RSessions)
                .Add(Operations)
                .Add(Connections);
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
                    ActiveWindow.Container.Show(focus: true, immediate: false);
                }

                // Reset the flag, so that further focus changes are not affected until the next debugger break occurs.
                _debuggerJustEnteredBreakMode = false;
            }
        }

        private void OnBrokerChanging(object sender, EventArgs e) {
            OnBrokerChangingAsync().DoNotWait();
        }

        private async Task OnBrokerChangingAsync() {
            await Shell.SwitchToMainThreadAsync();
            var factory = Shell.ExportProvider.GetExportedValueOrDefault<IInteractiveWindowComponentContainerFactory>();
            if (factory != null) {
                var component = await GetOrCreateVisualComponent(factory);
                component.Container.Show(focus: false, immediate: false);
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

            ActiveWindow = componentContainerFactory.Create(instanceId, evaluator, RSessions);
            var interactiveWindow = ActiveWindow.InteractiveWindow;
            interactiveWindow.TextView.Closed += (_, __) => evaluator.Dispose();
            _operations.InteractiveWindow = interactiveWindow;
            await interactiveWindow.InitializeAsync();
            ActiveWindow.Container.UpdateCommandStatus(true);
            return ActiveWindow;
        }

        public void Dispose() {
            _disposableBag.TryDispose();
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(IRSettings.CranMirror)) {
                SetMirrorToSession().DoNotWait();
            } else if (e.PropertyName == nameof(IRSettings.RCodePage)) {
                SetSessionCodePage().DoNotWait();
            }
        }

        private async Task SetMirrorToSession() {
            string mirrorName = _settings.CranMirror;
            string mirrorUrl = CranMirrorList.UrlFromName(mirrorName);

            foreach (var s in RSessions.GetSessions()) {
                try {
                    await s.SetVsCranSelectionAsync(mirrorUrl);
                } catch (RException) { } catch (OperationCanceledException) { }
            }
        }

        private async Task SetSessionCodePage() {
            var cp = _settings.RCodePage;

            foreach (var s in RSessions.GetSessions()) {
                try {
                    await s.SetCodePageAsync(cp);
                } catch (OperationCanceledException) { }
            }
        }
    }
}