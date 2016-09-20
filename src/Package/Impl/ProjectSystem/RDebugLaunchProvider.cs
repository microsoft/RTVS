// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Debugger;
using Microsoft.R.Debugger.PortSupplier;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.DebuggerProviders;
using Microsoft.VisualStudio.ProjectSystem.VS.Debuggers;
#else
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    // ExportDebugger must match rule name in ..\Rules\Debugger.xaml.
    [ExportDebugger("RDebugger")]
    [AppliesTo(Constants.RtvsProjectCapability)]
    internal class RDebugLaunchProvider : DebugLaunchProviderBase {
        private readonly ProjectProperties _properties;
        private readonly IRInteractiveWorkflow _interactiveWorkflow;

        [ImportingConstructor]
        public RDebugLaunchProvider(ConfiguredProject configuredProject, IRInteractiveWorkflowProvider interactiveWorkflowProvider)
            : base(configuredProject) {
            _properties = configuredProject.Services.ExportProvider.GetExportedValue<ProjectProperties>();
            _interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();
        }

        private IRSession Session => _interactiveWorkflow.RSession;

        public override Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions) {
            return Task.FromResult(true);
        }

        public override Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions) {
            var targets = new List<IDebugLaunchSettings>();

            if (Session.IsHostRunning) {
                uint pid = RDebugPortSupplier.GetProcessId(Session.Id);

                var target = new DebugLaunchSettings(launchOptions) {
                    LaunchOperation = DebugLaunchOperation.AlreadyRunning,
                    PortSupplierGuid = DebuggerGuids.PortSupplier,
                    PortName = RDebugPortSupplier.PortName,
                    LaunchDebugEngineGuid = DebuggerGuids.DebugEngine,
                    ProcessId = (int)pid,
                    Executable = RDebugPortSupplier.GetExecutableForAttach(pid),
                };

                targets.Add(target);
            }

            return Task.FromResult((IReadOnlyList<IDebugLaunchSettings>)targets);
        }

        public override async Task LaunchAsync(DebugLaunchOptions launchOptions) {
            // Reset first, before attaching debugger via LaunchAsync (since it'll detach on reset).
            if (await _properties.GetResetReplOnRunAsync()) {
                await _interactiveWorkflow.Operations.ResetAsync();
            }

            // Base implementation will try to launch or attach via the debugger, but there's nothing to launch
            // in case of Ctrl+F5 - we only want to source the file. So only invoke base if we intend to debug.
            if (!launchOptions.HasFlag(DebugLaunchOptions.NoDebug)) {
                await base.LaunchAsync(launchOptions);
            }

            _interactiveWorkflow.ActiveWindow?.Container.Show(false, immediate: false);

            string startupFile = await _properties.GetStartupFileAsync();

            if (string.IsNullOrEmpty(startupFile)) {
                _interactiveWorkflow.ActiveWindow?.InteractiveWindow.WriteErrorLine(Resources.Launch_NoStartupFile);
                return;
            }

            _interactiveWorkflow.Operations.SourceFileAsync(startupFile, echo: false)
                .SilenceException<Exception>()
                .DoNotWait();
        }
    }
}