// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Interpreters;
using Microsoft.Common.Core.Services;

namespace Microsoft.R.Host.Client.Test.Script {
    [ExcludeFromCodeCoverage]
    public class RHostScript : IDisposable {
        private IRSessionCallback _clientApp;
        private bool _disposed;

        public IRSession Session => Workflow.RSession;
        public IServiceContainer Services { get; }
        public IRInteractiveWorkflowProvider WorkflowProvider { get; }
        public IRInteractiveWorkflow Workflow { get; private set; }

        public static Version RVersion => new RInstallation().GetCompatibleEngines().First().Version;

        public RHostScript(IServiceContainer services, IRSessionCallback clientApp = null) : this(services, true) {
            _clientApp = clientApp;
            InitializeAsync().Wait();
        }

        public RHostScript(IServiceContainer services, bool async) {
            Services = services;
            WorkflowProvider = Services.GetService<IRInteractiveWorkflowProvider>();
            Workflow = WorkflowProvider.GetOrCreate();
        }

        public async Task InitializeAsync(IRSessionCallback clientApp = null) {
            _clientApp = clientApp;

            await Workflow.RSessions.TrySwitchBrokerAsync(GetType().Name);

            if (Workflow.RSession.IsHostRunning) {
                await Workflow.RSession.StopHostAsync();
            }

            await Workflow.RSession.StartHostAsync(new RHostStartupInfo(), _clientApp ?? new RHostClientTestApp(), 50000);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }

            if (disposing) {
                if (Session != null) {
                    Session.StopHostAsync().Wait(15000);
                    if (Session.IsHostRunning) {
                        Debugger.Launch();
                    }
                    Debug.Assert(!Session.IsHostRunning);
                }
                Workflow?.Dispose();
            }
            _disposed = true;
        }
    }
}
