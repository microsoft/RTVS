// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Containers;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Test.Fakes.ToolWindows;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Mef;

namespace Microsoft.R.Components.Test.Fakes.InteractiveWindow {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IRInteractiveWorkflowProvider))]
    [Export(typeof(IRInteractiveWorkflowVisualProvider))]
    [Export(typeof(TestRInteractiveWorkflowProvider))]
    [PartMetadata(PartMetadataAttributeNames.SkipInEditorTestCompositionCatalog, null)]
    public class TestRInteractiveWorkflowProvider : IRInteractiveWorkflowVisualProvider, IRInteractiveWorkflowProvider {
        private readonly IConnectionManagerProvider _connectionManagerProvider;
        private readonly IContainerManagerProvider _containerManagerProvider;
        private readonly IRHistoryProvider _historyProvider;
        private readonly IRPackageManagerProvider _packagesProvider;
        private readonly IRPlotManagerProvider _plotsProvider;
        private readonly ICoreShell _shell;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;

        private Lazy<IRInteractiveWorkflowVisual> _instanceLazy;
        public IRSessionCallback HostClientApp { get; set; }
        
        [ImportingConstructor]
        public TestRInteractiveWorkflowProvider(IRPackageManagerProvider packagesProvider
            , IRPlotManagerProvider plotsProvider
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , ICoreShell shell) 
            : this(shell.GetService<IConnectionManagerProvider>()
                  , shell.GetService<IContainerManagerProvider>()
                  , shell.GetService<IRHistoryProvider>()
                  , packagesProvider
                  , plotsProvider
                  , activeTextViewTracker
                  , debuggerModeTracker
                  , shell) {}

        public TestRInteractiveWorkflowProvider(IConnectionManagerProvider connectionManagerProvider
            , IContainerManagerProvider containerManagerProvider
            , IRHistoryProvider historyProvider
            , IRPackageManagerProvider packagesProvider
            , IRPlotManagerProvider plotsProvider
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , ICoreShell shell) {
            _connectionManagerProvider = connectionManagerProvider;
            _containerManagerProvider = containerManagerProvider;
            _historyProvider = historyProvider;
            _packagesProvider = packagesProvider;
            _plotsProvider = plotsProvider;
            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
            _shell = shell;
        }

        public void Dispose() {
            if (_instanceLazy?.IsValueCreated == true) {
                _instanceLazy?.Value?.Dispose();
            }
        }

        IRInteractiveWorkflowVisual IRInteractiveWorkflowVisualProvider.GetOrCreate() {
            Interlocked.CompareExchange(ref _instanceLazy, new Lazy<IRInteractiveWorkflowVisual>(CreateRInteractiveWorkflow), null);
            return _instanceLazy.Value;
        }

        IRInteractiveWorkflow IRInteractiveWorkflowProvider.GetOrCreate() => ((IRInteractiveWorkflowVisualProvider)this).GetOrCreate();

        public IRInteractiveWorkflow Active => _instanceLazy.IsValueCreated ? _instanceLazy.Value : null;

        private IRInteractiveWorkflowVisual CreateRInteractiveWorkflow() {
            var serviceManager = _shell.Services.Extend()
                .AddService<IRInteractiveWorkflowToolWindowService, TestRInteractiveWorkflowToolWindowService>();
            return new RInteractiveWorkflow(_connectionManagerProvider
                , _containerManagerProvider
                , _historyProvider
                , _packagesProvider
                , _plotsProvider
                , _activeTextViewTracker
                , _debuggerModeTracker
                , serviceManager);
        }
    }
}
