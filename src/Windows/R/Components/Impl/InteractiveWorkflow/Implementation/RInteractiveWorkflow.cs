// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ContainerManager;
using Microsoft.R.Components.Containers;
using Microsoft.R.Components.History;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Settings.Mirrors;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Platform.Interpreters;
using Microsoft.VisualStudio.R.Package.Repl;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public sealed class RInteractiveWorkflow : IRInteractiveWorkflowVisual {
        private readonly DisposableBag _disposableBag;
        private readonly IRSettings _settings;
        private readonly RInteractiveWorkflowOperations _operations;
        private readonly IMainThread _mainThread;
        private readonly IServiceManager _services;

        private TaskCompletionSource<IInteractiveWindowVisualComponent> _visualComponentTcs;

        public IServiceContainer Services => _services;
        public IRSession RSession { get; }

        public IConsole Console => _services.GetService<IConsole>();
        public IRSessionProvider RSessions => _services.GetService<IRSessionProvider>();
        public IConnectionManager Connections => _services.GetService<IConnectionManager>();
        public IContainerManager Containers => _services.GetService<IContainerManager>();
        public IRHistory History => _services.GetService<IRHistory>();
        public IRPackageManager Packages => _services.GetService<IRPackageManager>();
        public IRPlotManager Plots => _services.GetService<IRPlotManager>();
        public IRInteractiveWorkflowOperations Operations => _operations;
        public IRInteractiveWorkflowToolWindowService ToolWindows => _services.GetService<IRInteractiveWorkflowToolWindowService>();

        public IInteractiveWindowVisualComponent ActiveWindow { get; private set; }

        public event EventHandler<ActiveWindowChangedEventArgs> ActiveWindowChanged;

        public RInteractiveWorkflow(IConnectionManagerProvider connectionsProvider
            , IContainerManagerProvider containerProvider
            , IRHistoryProvider historyProvider
            , IRPackageManagerProvider packagesProvider
            , IRPlotManagerProvider plotsProvider
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , IServiceManager services) {
            _services = services
                .AddService<IRInteractiveWorkflow>(this)
                .AddService<IConsole, InteractiveWindowConsole>()
                .AddService<IRSessionProvider, RSessionProvider>()
                .AddService(s => connectionsProvider.CreateConnectionManager(this))
                .AddService(s => containerProvider.CreateContainerManager(this))
                .AddService(s => historyProvider.CreateRHistory(this))
                .AddService(s => packagesProvider.CreateRPackageManager(this))
                .AddService(s => plotsProvider.CreatePlotManager(this));

            _settings = _services.GetService<IRSettings>();
            _mainThread = _services.MainThread();
            _operations = new RInteractiveWorkflowOperations(this, debuggerModeTracker, Services);

            RSession = RSessions.GetOrCreate(SessionNames.InteractiveWindow);

            _settings.PropertyChanged += OnSettingsChanged;
            activeTextViewTracker.LastActiveTextViewChanged += LastActiveTextViewChanged;
            RSession.Disconnected += RSessionDisconnected;

            _disposableBag = DisposableBag.Create<RInteractiveWorkflow>()
                .Add(() => _settings.PropertyChanged -= OnSettingsChanged)
                .Add(() => activeTextViewTracker.LastActiveTextViewChanged -= LastActiveTextViewChanged)
                .Add(() => RSession.Disconnected -= RSessionDisconnected)
                .Add(Operations)
                .Add(_services);
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
            var factory = Services.GetService<IInteractiveWindowComponentContainerFactory>();
            var evaluator = new RInteractiveEvaluator(RSessions, RSession, History, Connections, Services, _settings, Console);

            var window = factory.Create(instanceId, evaluator, RSessions);
            var interactiveWindow = window.InteractiveWindow;
            interactiveWindow.TextView.Closed += (_, __) => evaluator.Dispose();
            _operations.InteractiveWindow = interactiveWindow;

            if (!RSessions.HasBroker) {
                var connectedToBroker = await Connections.TryConnectToPreviouslyUsedAsync();
                if (!connectedToBroker) {
                    var showConnectionsWindow = Connections.RecentConnections.Any();
                    if (!showConnectionsWindow) {
                        var message = Resources.NoLocalR.FormatInvariant(Environment.NewLine + Environment.NewLine,
                            Environment.NewLine);
                        var ui = Services.UI();
                        showConnectionsWindow = ui.ShowMessage(message, MessageButtons.YesNo) == MessageButtons.No;
                    }

                    if (!showConnectionsWindow) {
                        var installer = Services.GetService<IMicrosoftRClientInstaller>();
                        installer.LaunchRClientSetup(Services);
                    } else {
                        var toolWindows = Services.GetService<IRInteractiveWorkflowToolWindowService>();
                        toolWindows.Connections().Show(focus: false, immediate: false);
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
            } else if (e.PropertyName == nameof(IRSettings.GridDynamicEvaluation)) {
                RSession.SetGridEvalModeAsync(_settings.GridDynamicEvaluation).DoNotWait();
            }
        }

        private async Task SetMirrorToSession() {
            var mirrorName = _settings.CranMirror;
            var mirrorUrl = CranMirrorList.UrlFromName(mirrorName);

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
                } catch (RException) { } catch (OperationCanceledException) { }
            }
        }
    }
}