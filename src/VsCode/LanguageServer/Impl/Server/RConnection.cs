// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.LanguageServer.InteractiveWorkflow;

namespace Microsoft.R.LanguageServer.Server {
    /// <summary>
    /// Manages connection to RTVS
    /// </summary>
    internal sealed class RConnection : IDisposable {
        private IRInteractiveWorkflow _workflow;
        private IPackageIndex _packageIndex;

        public async Task ConnectAsync(IServiceContainer services, CancellationToken ct) {
            var provider = services.GetService<IRInteractiveWorkflowProvider>();
            _workflow = provider.GetOrCreate();

            var path = @"C:\Program Files\R\R-3.4.0";
            var log = services.Log();
            var info = BrokerConnectionInfo.Create(services.Security(), "VSCR", path, string.Empty, false);

            log.Write(LogVerbosity.Normal, MessageCategory.General, "Switching local broker");
            if (await _workflow.RSessions.TrySwitchBrokerAsync("VSCR", info, ct)) {
                try {
                    log.Write(LogVerbosity.Normal, MessageCategory.General, $"Starting R Host with {path}");
                    await _workflow.RSession.StartHostAsync(new RHostStartupInfo(), new RSessionCallback(), Debugger.IsAttached ? 100000 : 20000, ct);
                } catch(Exception ex) {
                    log.Write(LogVerbosity.Normal, MessageCategory.Error, $"Unable to start host. Exception: {ex.Message}");
                    return;
                }

                // Start package building
                log.Write(LogVerbosity.Normal, MessageCategory.General, "Starting package index build");
                _packageIndex = services.GetService<IPackageIndex>();
                _packageIndex.BuildIndexAsync(ct).ContinueWith(t => {
                    log.Write(LogVerbosity.Normal, MessageCategory.General, $"Package index build complete");
                }).DoNotWait();
            } else {
                log.Write(LogVerbosity.Normal, MessageCategory.Error, "Unable to connect to the local broker");
            }
        }

        public void Dispose() => _workflow?.Dispose();
    }
}
