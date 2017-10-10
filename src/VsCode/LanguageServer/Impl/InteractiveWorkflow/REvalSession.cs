// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;

namespace Microsoft.R.LanguageServer.InteractiveWorkflow {
    internal sealed class REvalSession : IREvalSession {
        private readonly IServiceContainer _services;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly CancellationTokenSource _hostStartCts = new CancellationTokenSource();
        private readonly RSessionCallback _sessionCallback;
        private IRSession _session;
        private readonly Task _sessionStart;
        private StringBuilder _output;
        private StringBuilder _errors;

        public REvalSession(IServiceContainer services) {
            _services = services;
            _workflow = services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();

            _sessionCallback = new RSessionCallback {
                PlotDeviceProperties = new PlotDeviceProperties(800, 600, 96)
            };

            _sessionStart = StartSessionAsync(_hostStartCts.Token);
        }

        #region IREvalSession
        public async Task<string> ExecuteCodeAsync(string code, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();
            var result = string.Empty;

            try {
                ct.ThrowIfCancellationRequested();
                var session = await StartSessionAsync(ct);

                session.Output += OnSessionOutput;
                await ExecuteAndCaptureOutputAsync(session, code, ct);

                if (_sessionCallback.PlotResult != null) {
                    result = Convert.ToBase64String(_sessionCallback.PlotResult);
                    _sessionCallback.PlotResult = null;
                } else if (_output != null && _output.Length > 0) {
                    result = _output.ToString();
                } else if (_errors != null && _errors.Length > 0) {
                    result = _errors.ToString();
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _output = _errors = null;
                result = ex.Message;
            } finally {
                if (_session != null) {
                    _session.Output -= OnSessionOutput;
                }
            }

            return result;
        }
        public Task InterruptAsync() {
            try {
                return _session.CancelAllAsync();
            } catch (OperationCanceledException) { }
            return Task.CompletedTask;
        }

        public async Task ResetAsync() {
            try {
                await _session.StopHostAsync(true);
                await StartSessionAsync(_hostStartCts.Token);
            } catch(OperationCanceledException) { }
        }

        public Task CancelAsync() {
            try {
                return _session.CancelAllAsync();
            } catch (OperationCanceledException) { }
            return Task.CompletedTask;
        }
        #endregion

        private async Task ExecuteAndCaptureOutputAsync(IRSession session, string expression, CancellationToken cancellationToken) {
            _output = new StringBuilder();
            _errors = new StringBuilder();

            try {
                using (var inter = await session.BeginInteractionAsync(true, cancellationToken)) {
                    await inter.RespondAsync(expression);
                }
            } catch (OperationCanceledException) { }
        }

        private void OnSessionOutput(object sender, ROutputEventArgs e) {
            if (_output != null && _errors != null) {
                if (e.OutputType == OutputType.Error) {
                    _errors.Append(e.Message);
                } else {
                    _output.Append(e.Message);
                }
            }
        }

        private async Task<IRSession> StartSessionAsync(CancellationToken ct) {
            if (_session == null) {
                _session = _workflow.RSessions.GetOrCreate("VSCR_Output");
            } else {
                await _sessionStart;
            }

            if (!_session.IsHostRunning) {
                await _session.EnsureHostStartedAsync(new RHostStartupInfo(isInteractive: true), _sessionCallback, 3000, ct);
                await _session.SetVsGraphicsDeviceAsync();
            }

            return _session;
        }

        public Task StopSessionAsync()
            => _session?.StopHostAsync(waitForShutdown: false) ?? Task.CompletedTask;

        public void Dispose() {
            _hostStartCts.Cancel();
            _session?.Dispose();
        }
    }
}
