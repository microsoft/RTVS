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

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export(typeof(IRInteractiveWorkflowProvider))]
    [Export(typeof(IRInteractiveWorkflowVisualProvider))]
    internal class VsRInteractiveWorkflowProvider : IRInteractiveWorkflowVisualProvider, IRInteractiveWorkflowProvider, IDisposable {
        private readonly DisposableBag _disposableBag = DisposableBag.Create<VsRInteractiveWorkflowProvider>();

        private readonly IConnectionManagerProvider _connectionsProvider;
        private readonly IRHistoryProvider _historyProvider;
        private readonly IRPackageManagerProvider _packagesProvider;
        private readonly IRPlotManagerProvider _plotsProvider;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;
        private readonly IApplication _app;
        private readonly ICoreShell _shell;

        private Lazy<IRInteractiveWorkflowVisual> _instanceLazy;

        [ImportingConstructor]
        public VsRInteractiveWorkflowProvider(ICoreShell shell) {
            _shell = shell;

            _connectionsProvider = shell.GetService<IConnectionManagerProvider>();
            _historyProvider = shell.GetService<IRHistoryProvider>();
            _packagesProvider = shell.GetService<IRPackageManagerProvider>();
            _plotsProvider = shell.GetService<IRPlotManagerProvider>();
            _activeTextViewTracker = shell.GetService<IActiveWpfTextViewTracker>();
            _debuggerModeTracker = shell.GetService<IDebuggerModeTracker>();
            _connectionsProvider = shell.GetService<IConnectionManagerProvider>();

            _app = _shell.GetService<IApplication>();
            _app.Terminating += OnApplicationTerminating;
        }

        private void OnApplicationTerminating(object sender, EventArgs e) => Dispose();

        public void Dispose() {
            _app.Terminating -= OnApplicationTerminating;
            _disposableBag.TryDispose();
        }

        IRInteractiveWorkflowVisual IRInteractiveWorkflowVisualProvider.GetOrCreate() { 
            _disposableBag.ThrowIfDisposed();
            Interlocked.CompareExchange(ref _instanceLazy, new Lazy<IRInteractiveWorkflowVisual>(CreateRInteractiveWorkflow), null);
            return _instanceLazy.Value;
        }

        IRInteractiveWorkflow IRInteractiveWorkflowProvider.GetOrCreate() => ((IRInteractiveWorkflowVisualProvider)this).GetOrCreate();

        public IRInteractiveWorkflow Active => (_instanceLazy != null && _instanceLazy.IsValueCreated) ? _instanceLazy.Value : null;

        private IRInteractiveWorkflowVisual CreateRInteractiveWorkflow() {
            _disposableBag.Add(DisposeInstance);
            return new RInteractiveWorkflow(_connectionsProvider, _historyProvider, _packagesProvider, _plotsProvider, _activeTextViewTracker, _debuggerModeTracker, _shell);
        }

        private void DisposeInstance() {
            var lazy = Interlocked.Exchange(ref _instanceLazy, null);
            if (lazy != null && lazy.IsValueCreated) {
                lazy.Value.Dispose();
            }
        }
    }
}