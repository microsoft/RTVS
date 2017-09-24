// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LanguageServer.VsCode.Contracts.Client;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.LanguageServer.InteractiveWorkflow;
using Microsoft.R.LanguageServer.Services;

namespace Microsoft.R.LanguageServer.Server {
    /// <summary>
    /// Manages connection to RTVS
    /// </summary>
    internal sealed class RConnection : IDisposable {
        private readonly IServiceContainer _services;
        private IRInteractiveWorkflow _workflow;
        private IPackageIndex _packageIndex;
        private IOutput _output;

        public RConnection(IServiceContainer services) {
            _services = services;
        }

        public async Task ConnectAsync(CancellationToken ct) {
            var provider = _services.GetService<IRInteractiveWorkflowProvider>();
            _workflow = provider.GetOrCreate();
            _output = _services.GetService<IOutput>();

            var path = @"C:\Program Files\R\R-3.4.0";
            var log = _services.Log();
            var info = BrokerConnectionInfo.Create(_services.Security(), "VSCR", path, string.Empty, false);

            var start = DateTime.Now;
            var message = $"Starting R Process with {path}...";
            _output.Write(message);

            log.Write(LogVerbosity.Normal, MessageCategory.General, "Switching local broker");
            if (await _workflow.RSessions.TrySwitchBrokerAsync("VSCR", info, ct)) {
                try {
                    await _workflow.RSession.StartHostAsync(new RHostStartupInfo(), new RSessionCallback(), Debugger.IsAttached ? 100000 : 20000, ct);
                } catch(Exception ex) {
                     _output.WriteError($"Unable to start R process. Exception: {ex.Message}");
                    return;
                }

                // Start package building
                _output.Write($"complete in {(DateTime.Now - start).TotalMilliseconds}");
                start = DateTime.Now;
                _output.Write("Building IntelliSense index...");

                _packageIndex = _services.GetService<IPackageIndex>();
                _packageIndex.BuildIndexAsync(ct).ContinueWith(t => {
                    _output.Write($"complete in {(DateTime.Now - start).TotalMilliseconds}");
                }).DoNotWait();
            } else {
                _output.WriteError("Unable to start R process");
            }
        }

        public void Dispose() => _workflow?.Dispose();
    }
}
