// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.History;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Settings.Mirrors;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.R.Package.Repl;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public sealed class RInteractiveWorkflow : IRInteractiveWorkflowVisual {
        private readonly DisposableBag _disposableBag;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;
        private readonly IRSettings _settings;
        private readonly RInteractiveWorkflowOperations _operations;
        private readonly IMainThread _mainThread;

        private TaskCompletionSource<IInteractiveWindowVisualComponent> _visualComponentTcs;

        public ICoreShell Shell { get; }
        public IConnectionManager Connections { get; }
        public IRHistory History { get; }
        public IRSessionProvider RSessions { get; }
        public IRSession RSession { get; }
        public IRPackageManager Packages { get; }
        public IRPlotManager Plots { get; }
        public IConsole Console { get; }

        public IRInteractiveWorkflowOperations Operations => _operations;

        public IInteractiveWindowVisualComponent ActiveWindow { get; private set; }

        public event EventHandler<ActiveWindowChangedEventArgs> ActiveWindowChanged;

        public RInteractiveWorkflow(IConnectionManagerProvider connectionsProvider
            , IRHistoryProvider historyProvider
            , IRPackageManagerProvider packagesProvider
            , IRPlotManagerProvider plotsProvider
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , ICoreShell coreShell) {

            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
            _settings = coreShell.GetService<IRSettings>();
            _mainThread = coreShell.MainThread();

            Shell = coreShell;
            var console = new InteractiveWindowConsole(this);
            Console = console;
            RSessions = new RSessionProvider(coreShell.Services, Console);

            RSession = RSessions.GetOrCreate(SessionNames.InteractiveWindow);
            Connections = connectionsProvider.CreateConnectionManager(this);

            History = historyProvider.CreateRHistory(this);
            Packages = packagesProvider.CreateRPackageManager(_settings, this);
            Plots = plotsProvider.CreatePlotManager(_settings, this, coreShell.FileSystem());
            _operations = new RInteractiveWorkflowOperations(this, _debuggerModeTracker, Shell);

            _activeTextViewTracker.LastActiveTextViewChanged += LastActiveTextViewChanged;
            RSession.Disconnected += RSessionDisconnected;

            _settings.PropertyChanged += OnSettingsChanged;

            _disposableBag = DisposableBag.Create<RInteractiveWorkflow>()
                .Add(() => _settings.PropertyChanged -= OnSettingsChanged)
                .Add(() => _activeTextViewTracker.LastActiveTextViewChanged -= LastActiveTextViewChanged)
                .Add(() => RSession.Disconnected -= RSessionDisconnected)
                .Add(RSessions)
                .Add(Operations)
                .Add(Connections)
                .Add(console);
        }

        private void LastActiveTextViewChanged(object sender, ActiveTextViewChangedEventArgs e) {
            if (ActiveWindow == null) {
                return;
            }

            if (ActiveWindow.TextView.HasAggregateFocus) {
                _mainThread.Post(Operations.PositionCaretAtPrompt);
            }
        }

        private void RSessionDisconnected(object o, EventArgs eventArgs) {
            Operations.ClearPendingInputs();
            ActiveWindow?.Container.UpdateCommandStatus(false);
        }

        public Task<IInteractiveWindowVisualComponent> GetOrCreateVisualComponentAsync(int instanceId = 0) {
            _mainThread.CheckAccess();

            if (_visualComponentTcs == null) {
                _visualComponentTcs = new TaskCompletionSource<IInteractiveWindowVisualComponent>();
                CreateVisualComponentAsync(instanceId).DoNotWait();
            } else if (instanceId != 0) {
                // Right now only one instance of interactive window is allowed
                throw new InvalidOperationException("Right now only one instance of interactive window is allowed");
            }

            return _visualComponentTcs.Task;
        }

        private async Task CreateVisualComponentAsync(int instanceId) {
            var factory = Shell.GetService<IInteractiveWindowComponentContainerFactory>();
            var evaluator = new RInteractiveEvaluator(RSessions, RSession, History, Connections, Shell, _settings, new InteractiveWindowConsole(this));

            var window = factory.Create(instanceId, evaluator, RSessions);
            var interactiveWindow = window.InteractiveWindow;
            interactiveWindow.TextView.Closed += (_, __) => evaluator.Dispose();
            _operations.InteractiveWindow = interactiveWindow;

            if (!RSessions.HasBroker) {
                var connectedToBroker = await Connections.TryConnectToPreviouslyUsedAsync();
                if (!connectedToBroker) {
                    var showConnectionsWindow = Connections.RecentConnections.Any();
                    if (!showConnectionsWindow) {
                        var message = Resources.NoLocalR.FormatInvariant(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                        var ui = Shell.UI();
                        showConnectionsWindow = ui.ShowMessage(message, MessageButtons.YesNo) == MessageButtons.No;
                    }

                    if (!showConnectionsWindow) {
                        var installer = Shell.GetService<IMicrosoftRClientInstaller>();
                        installer.LaunchRClientSetup(Shell.Services);
                    } else {
                        var cmv = Connections as IConnectionManagerVisual;
                        cmv.GetOrCreateVisualComponent().Container.Show(focus: false, immediate: false);
                    }
                }
            }

            await interactiveWindow.InitializeAsync();
            RSession.RestartOnBrokerSwitch = true;

            ActiveWindow = window;
            ActiveWindow.Container.UpdateCommandStatus(true);
            _visualComponentTcs.SetResult(ActiveWindow);
            ActiveWindowChanged?.Invoke(this, new ActiveWindowChangedEventArgs(window));
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