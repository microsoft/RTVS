// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export(typeof(IRInteractiveWorkflowProvider))]
    internal class VsRInteractiveWorkflowProvider : IRInteractiveWorkflowProvider {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRHistoryProvider _historyProvider;
        private readonly IRPackageManagerProvider _pacakagesProvider;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IDebuggerModeTracker _debuggerModeTracker;

        private Lazy<IRInteractiveWorkflow> _instanceLazy;

        [ImportingConstructor]
        public VsRInteractiveWorkflowProvider(IRSessionProvider sessionProvider
            , IRHistoryProvider historyProvider
            , IRPackageManagerProvider pacakagesProvider
            , IActiveWpfTextViewTracker activeTextViewTracker
            , IDebuggerModeTracker debuggerModeTracker) {

            _sessionProvider = sessionProvider;
            _historyProvider = historyProvider;
            _pacakagesProvider = pacakagesProvider;
            _activeTextViewTracker = activeTextViewTracker;
            _debuggerModeTracker = debuggerModeTracker;
        }

        public IRInteractiveWorkflow GetOrCreate() {
            Interlocked.CompareExchange(ref _instanceLazy, new Lazy<IRInteractiveWorkflow>(CreateRInteractiveWorkflow),null);
            return _instanceLazy.Value;
        }
        
        private IRInteractiveWorkflow CreateRInteractiveWorkflow() {
            var shell = EditorShell.Current;
            var settings = RToolsSettings.Current;
            return new RInteractiveWorkflow(_sessionProvider, _historyProvider, _pacakagesProvider, _activeTextViewTracker, _debuggerModeTracker, RHostClientApp.Instance, shell, settings, DisposeInstance);
        }

        private void DisposeInstance() {
            _instanceLazy = null;
        }
    }
}