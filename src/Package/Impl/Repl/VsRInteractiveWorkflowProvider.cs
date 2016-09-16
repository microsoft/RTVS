// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Workspace;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings;

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export(typeof(IRInteractiveWorkflowProvider))]
    internal class VsRInteractiveWorkflowProvider : IRInteractiveWorkflowProvider, IDisposable {
        private readonly DisposableBag _disposableBag = DisposableBag.Create<VsRInteractiveWorkflowProvider>();

        private readonly IConnectionManagerProvider _connectionsProvider;
        private readonly IRHistoryProvider _historyProvider;
        private readonly IRPackageManagerProvider _packagesProvider;
        private readonly IRPlotManagerProvider _plotsProvider;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;
        private readonly ICoreShell _shell;
        private readonly IWorkspaceServices _wss;

        private Lazy<IRInteractiveWorkflow> _instanceLazy;

        [ImportingConstructor]
        public VsRInteractiveWorkflowProvider(IConnectionManagerProvider connectionsProvider
            , IRHistoryProvider historyProvider
            , IRPackageManagerProvider packagesProvider
            , IRPlotManagerProvider plotsProvider
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , ICoreShell shell
            , IWorkspaceServices wss) {

            _connectionsProvider = connectionsProvider;
            _historyProvider = historyProvider;
            _packagesProvider = packagesProvider;
            _plotsProvider = plotsProvider;
            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
            _shell = shell;
            _wss = wss;
        }

        public void Dispose() {
            _disposableBag.TryMarkDisposed();
        }

        public IRInteractiveWorkflow GetOrCreate() {
            _disposableBag.ThrowIfDisposed();

            Interlocked.CompareExchange(ref _instanceLazy, new Lazy<IRInteractiveWorkflow>(CreateRInteractiveWorkflow), null);
            return _instanceLazy.Value;
        }

        public IRInteractiveWorkflow Active => (_instanceLazy != null && _instanceLazy.IsValueCreated) ? _instanceLazy.Value : null;

        private IRInteractiveWorkflow CreateRInteractiveWorkflow() {
            var settings = RToolsSettings.Current;
            var sessionProvider = new RSessionProvider(new RSessionProviderCallback( _shell, _instanceLazy));
            var workflow = new RInteractiveWorkflow(sessionProvider, _connectionsProvider, _historyProvider, _packagesProvider, 
                                                    _plotsProvider, _activeTextViewTracker, _debuggerModeTracker, 
                                                    _shell, settings, _wss, () => DisposeInstance(sessionProvider));
            _disposableBag.Add(workflow);

            sessionProvider.BrokerChanging += OnBrokerChanging;
            return workflow;
        }

        private void OnBrokerChanging(object sender, EventArgs e) {
            _instanceLazy.Value?.ActiveWindow?.Container?.Show(focus: true, immediate: false);
        }

        private void DisposeInstance(IRSessionProvider sessionProvider) {
            sessionProvider.BrokerChanging -= OnBrokerChanging;
            sessionProvider.Dispose();
            _instanceLazy = null;
        }
    }
}