// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Mef;

namespace Microsoft.R.Components.Test.Fakes.InteractiveWindow {
    [Export(typeof(IRInteractiveWorkflowProvider))]
    [Export(typeof(TestRInteractiveWorkflowProvider))]
    [PartMetadata(PartMetadataAttributeNames.SkipInEditorTestCompositionCatalog, null)]
    public class TestRInteractiveWorkflowProvider : IRInteractiveWorkflowProvider {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRHistoryProvider _historyProvider;
        private readonly IRPackageManagerProvider _packagesProvider;
        private readonly IRPlotManagerProvider _plotsProvider;
        private readonly ICoreShell _shell;
        private readonly IRSettings _settings;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;

        private Lazy<IRInteractiveWorkflow> _instanceLazy;
        public IRSessionCallback HostClientApp { get; set; }

        [ImportingConstructor]
        public TestRInteractiveWorkflowProvider(IRSessionProvider sessionProvider
            , IRHistoryProvider historyProvider
            , IRPackageManagerProvider packagesProvider
            , IRPlotManagerProvider plotsProvider
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker
            , ICoreShell shell
            , IRSettings settings) {

            _sessionProvider = sessionProvider;
            _historyProvider = historyProvider;
            _packagesProvider = packagesProvider;
            _plotsProvider = plotsProvider;
            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
            _shell = shell;
            _settings = settings;
        }

        public IRInteractiveWorkflow GetOrCreate() {
            Interlocked.CompareExchange(ref _instanceLazy, new Lazy<IRInteractiveWorkflow>(CreateRInteractiveWorkflow), null);
            return _instanceLazy.Value;
        }
        
        private IRInteractiveWorkflow CreateRInteractiveWorkflow() {
            return new RInteractiveWorkflow(_sessionProvider, _historyProvider, _packagesProvider, _plotsProvider, _activeTextViewTracker, _debuggerModeTracker, _shell, _settings, DisposeInstance);
        }

        private void DisposeInstance() {
            _instanceLazy = null;
        }
    }
}
