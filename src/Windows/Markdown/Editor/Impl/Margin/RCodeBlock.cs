// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.R.Host.Client;
using static System.FormattableString;

namespace Microsoft.Markdown.Editor.Margin {
    internal enum CodeBlockEvalState {
        Created,
        Evaluated,
        Rendered
    }
    internal sealed class RCodeBlock {
        private readonly string _text;

        private StringBuilder _output;
        private StringBuilder _errors;

        public int BlockNumber { get; }
        public int Hash { get; }
        public string Result { get; set; }
        public CodeBlockEvalState State { get; set; } = CodeBlockEvalState.Created;

        public string HtmlElementId => Invariant($"rcode_{BlockNumber}_{Hash}");

        public RCodeBlock(int number, string text, int hash) {
            BlockNumber = number;
            Hash = hash;
            _text = text;
        }

        public async Task<string> EvaluateAsync(IRSession session, RSessionCallback callback, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            try {
                session.Output += OnSessionOutput;
                await ExecuteAndCaptureOutputAsync(session, _text, ct);

                if (callback.PlotResult != null) {
                    Result = Invariant($"<img src='data:image/gif;base64, {Convert.ToBase64String(callback.PlotResult)}' />");
                    callback.PlotResult = null;
                } else if(_output.Length > 0) {
                    Result = Invariant($"<code style='white-space: pre-wrap'>{_output.ToString()}</code>");
                } else if (_errors.Length > 0) {
                    Result = Invariant($"<code style='white-space: pre-wrap; color: red'>{_errors.ToString()}</code>");
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _output = _errors = null;
                session.Output -= OnSessionOutput;
                Result = ex.Message;
            }
            State = CodeBlockEvalState.Evaluated;
            return Result;
        }

        private async Task ExecuteAndCaptureOutputAsync(IRSession session, string expression, CancellationToken cancellationToken) {
            _output = new StringBuilder();
            _errors = new StringBuilder();

            using (var inter = await session.BeginInteractionAsync(isVisible: true, cancellationToken: cancellationToken)) {
                await inter.RespondAsync(expression);
            }
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
    }
}
