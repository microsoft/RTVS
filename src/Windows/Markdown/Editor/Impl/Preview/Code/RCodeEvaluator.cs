// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client;
using static System.FormattableString;
using Timer = System.Timers.Timer;

namespace Microsoft.Markdown.Editor.Preview.Code {
    internal sealed class RCodeEvaluator : IDisposable {
        private const int InactivityTimeout = 5 * 60 * 1000; // 5 minutes

        private readonly Timer _activityTimer;

        private StringBuilder _output;
        private StringBuilder _errors;

        public RCodeEvaluator(string documentName, IServiceContainer services) {
            EvalSession = new EvalSession(documentName, services);

            _activityTimer = new Timer(InactivityTimeout);
            _activityTimer.Elapsed += OnActivityTimerElapsed;
        }

        internal EvalSession EvalSession { get; }

        public async Task EvaluateBlocksAsync(IEnumerable<RCodeBlock> blocks, CancellationToken ct) {
            var blocksArray = blocks.ToArray();
            await TaskUtilities.SwitchToBackgroundThread();

            foreach (var block in blocksArray.Where(b => b.State == CodeBlockState.Created)) {
                await EvaluateBlockAsync(block, ct);
            }
        }

        public async Task<string> EvaluateBlockAsync(RCodeBlock block, CancellationToken ct) {
            block.State = CodeBlockState.Created;
            await TaskUtilities.SwitchToBackgroundThread();

            try {
                ct.ThrowIfCancellationRequested();
                var session = await EvalSession.StartSessionAsync(ct);
                var callback = EvalSession.SessionCallback;

                session.Output += OnSessionOutput;
                await ExecuteAndCaptureOutputAsync(session, block.Text, ct);

                if (callback.PlotResult != null) {
                    block.Result = Invariant($"<img src='data:image/gif;base64, {Convert.ToBase64String(callback.PlotResult)}' style='display:block; margin: 0 auto; text-align: center;' />");
                    callback.PlotResult = null;
                } else if (_output != null && _output.Length > 0) {
                    block.Result = block.DisplayMessages ? HtmlFormatter.FormatCode(_output.ToString()) : string.Empty;
                } else if (_errors != null && _errors.Length > 0) {
                    block.Result = block.DisplayMessages && block.DisplayErrors ? FormatError(_errors.ToString()) : string.Empty;
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _output = _errors = null;
                block.Result = FormatError(ex.Message);
            } finally {
                if (EvalSession.RSession != null) {
                    EvalSession.RSession.Output -= OnSessionOutput;
                }
                SignalActivity();
            }

            block.State = CodeBlockState.Evaluated;
            return block.Result;
        }

        private string FormatError(string error) => HtmlFormatter.FormatCode(error, "color: red;");

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

        private void SignalActivity() {
            _activityTimer.Stop();
            _activityTimer.Start();
        }

        private void OnActivityTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
            if (EvalSession.RSession != null && EvalSession.RSession.IsHostRunning) {
                EvalSession.StopSessionAsync().DoNotWait();
            }
        }

        public void Dispose() {
            _activityTimer.Dispose();
            EvalSession.Dispose();
        }
    }
}
