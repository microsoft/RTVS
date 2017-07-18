// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using static System.FormattableString;

namespace Microsoft.Markdown.Editor.Preview.Code {
    internal sealed class EvalSession : IDisposable {
        private readonly IServiceContainer _services;
        private readonly CancellationTokenSource _hostStartCts = new CancellationTokenSource();
        private readonly string _sessionId;
        private readonly Task _sessionStart;

        public EvalSession(string documentName, IServiceContainer services) {
            _services = services;
            _sessionId = Invariant($"{documentName} - {Guid.NewGuid()}");
            _sessionStart = StartSessionAsync(_hostStartCts.Token);

            SessionCallback = new RSessionCallback {
                PlotDeviceProperties = new PlotDeviceProperties(800, 600, 96)
            };
        }

        public IRSession RSession { get; private set; }

        public RSessionCallback SessionCallback { get; internal set; }

        public async Task<IRSession> StartSessionAsync(CancellationToken ct) {
            if (RSession == null) {
                var workflow = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
                RSession = workflow.RSessions.GetOrCreate(_sessionId);
            } else {
                await _sessionStart;
            }

            if (!RSession.IsHostRunning) {
                var settings = _services.GetService<IRSettings>();
                await RSession.EnsureHostStartedAsync(
                    new RHostStartupInfo(settings.CranMirror, codePage: settings.RCodePage), SessionCallback, 3000, ct);
            }

            return RSession;
        }

        public Task StopSessionAsync()
            => RSession?.StopHostAsync(waitForShutdown: false) ?? Task.CompletedTask;

        public void Dispose() {
            _hostStartCts.Cancel();
            RSession?.Dispose();
        }
    }
}
