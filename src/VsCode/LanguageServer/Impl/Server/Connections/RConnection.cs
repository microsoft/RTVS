// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
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
using Microsoft.R.LanguageServer.Logging;
using Microsoft.R.LanguageServer.Server.Settings;
using Microsoft.R.LanguageServer.Settings;
using Microsoft.R.Platform.Interpreters;

namespace Microsoft.R.LanguageServer.Server {
    /// <summary>
    /// Manages connection to RTVS
    /// </summary>
    internal sealed class RConnection : IDisposable {
        private readonly IServiceContainer _services;
        private readonly CancellationToken _cancellationToken;
        private IRInteractiveWorkflow _workflow;
        private IPackageIndex _packageIndex;
        private IOutput _output;

        public RConnection(IServiceContainer services, CancellationToken cancellationToken) {
            _services = services;
            _cancellationToken = cancellationToken;

            var settings = _services.GetService<ISettingsManager>();
            settings.SettingsChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object s, EventArgs e) {
            var settings = _services.GetService<ISettingsManager>();
            settings.SettingsChanged -= OnSettingsChanged;
            ConnectAsync(_cancellationToken).DoNotWait();
        }

        private async Task ConnectAsync(CancellationToken ct) {
            var provider = _services.GetService<IRInteractiveWorkflowProvider>();
            _workflow = provider.GetOrCreate();
            _output = _services.GetService<IOutput>();

            var e = GetREngine();
             var log = _services.Log();
            var info = BrokerConnectionInfo.Create(_services.Security(), "VSCR", e.InstallPath, string.Empty, false);

            var start = DateTime.Now;
            var message = $"Starting R Process with {e.InstallPath}...";
            _output.Write(message);

            log.Write(LogVerbosity.Normal, MessageCategory.General, $"Switching local broker to {e.InstallPath}");
            if (await _workflow.RSessions.TrySwitchBrokerAsync("VSCR", info, ct)) {
                try {
                    await _workflow.RSession.StartHostAsync(new RHostStartupInfo(), new RSessionCallback(), Debugger.IsAttached ? 100000 : 20000, ct);
                } catch (Exception ex) {
                    _output.WriteError($"Unable to start R process. Exception: {ex.Message}");
                    return;
                }

                // Start package building
                _output.Write($"complete in {(DateTime.Now - start)}");
                start = DateTime.Now;
                _output.Write("Building IntelliSense index...");

                _packageIndex = _services.GetService<IPackageIndex>();
                _packageIndex.BuildIndexAsync(ct).ContinueWith(t => {
                    _output.Write($"complete in {(DateTime.Now - start)}");
                }, ct).DoNotWait();
            } else {
                _output.WriteError("Unable to start R process");
            }
        }

        public void Dispose() => _workflow?.Dispose();

        private IRInterpreterInfo GetREngine() {
            var ris = _services.GetService<IRInstallationService>();
            var engines = ris.GetCompatibleEngines(new SupportedRVersionRange(3, 2, 3, 9)).ToList();
            if(engines.Count == 0) {
                const string message = "Unable to find R intepreter.";
                _output.Write(message + " Terminating.");
                throw new InvalidOperationException(message);
            }

            _output.Write("Available R interpreters:");
            for (var i = 0; i < engines.Count; i++) {
                _output.Write($"\t[{i}] {engines[i].Name}");
            }
            _output.Write("You can specify the desired interpreter index in the R settings");

            var rs = _services.GetService<IREngineSettings>();
            if(rs.InterpreterIndex < 0 || rs.InterpreterIndex > engines.Count) {
                _output.Write($"WARNING: selected interpreter [{rs.InterpreterIndex}] does not exist. Using [0] instead");
                rs.InterpreterIndex = 0;
            } else {
                _output.Write($"Selected interpreter: [{rs.InterpreterIndex}] {engines[rs.InterpreterIndex].Name}.\n");
            }

            return engines[rs.InterpreterIndex];
        }
    }
}
