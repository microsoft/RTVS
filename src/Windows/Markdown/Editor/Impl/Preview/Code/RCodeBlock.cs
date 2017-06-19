// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Host.Client;
using static System.FormattableString;

namespace Microsoft.Markdown.Editor.Preview.Code {
    internal sealed class RCodeBlock {
        private readonly string _text;

        private StringBuilder _output;
        private StringBuilder _errors;

        public int BlockNumber { get; }
        public int Hash { get; }
        public string Result { get; set; }
        public CodeBlockState State { get; set; } = CodeBlockState.Created;
        public bool Eval { get; private set; } = true;
        public bool DisplayErrors { get; private set; } = true;
        public bool DisplayWarnings { get; private set; } = true;
        public bool EchoContent { get; private set; } = true;

        public string HtmlElementId => Invariant($"rcode_{BlockNumber}_{Hash}");

        public RCodeBlock(int number, string arguments, string text, int hash) {
            BlockNumber = number;
            Hash = hash;
            _text = text;
            ExtractOptions(arguments);
        }

        public async Task<string> EvaluateAsync(IRSession session, RSessionCallback callback, CancellationToken ct) {
            try {
                ct.ThrowIfCancellationRequested();

                session.Output += OnSessionOutput;
                await ExecuteAndCaptureOutputAsync(session, _text, ct);

                if (callback.PlotResult != null) {
                    Result = Invariant($"<img src='data:image/gif;base64, {Convert.ToBase64String(callback.PlotResult)}' />");
                    callback.PlotResult = null;
                } else if (_output.Length > 0) {
                    Result = Invariant($"<code style='white-space: pre-wrap'>{_output.ToString()}</code>");
                } else if (_errors.Length > 0) {
                    Result = DisplayErrors ? FormatError(_errors.ToString()) : string.Empty;
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _output = _errors = null;
                Result = FormatError(ex.Message);
            } finally {
                session.Output -= OnSessionOutput;
            }
            State = CodeBlockState.Evaluated;
            return Result;
        }

        private string FormatError(string error)
            => Invariant($"<code style='white-space: pre-wrap; color: red'>{error}</code>");

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
        private void ExtractOptions(string info) {
            if (string.IsNullOrEmpty(info)) {
                return;
            }
            // Lookup 'echo=TRUE'
            var tokens = new TokenStream<RToken>(new RTokenizer().Tokenize(info), RToken.EndOfStreamToken);
            while (!tokens.IsEndOfStream()) {
                var t = tokens.CurrentToken;
                if (t.TokenType == RTokenType.Identifier) {
                    if (t.Length == 4) {
                        if (info.Substring(t.Start, t.Length).EqualsOrdinal("echo")) {
                            EchoContent = GetTokenValue(info, tokens, true);
                        } else if (info.Substring(t.Start, t.Length).EqualsOrdinal("eval")) {
                            Eval = GetTokenValue(info, tokens, true);
                        }
                    } else if (t.Length == 5 && info.Substring(t.Start, t.Length).EqualsOrdinal("error")) {
                        DisplayErrors = GetTokenValue(info, tokens, true);
                    } else if (t.Length == 7 && info.Substring(t.Start, t.Length).EqualsOrdinal("warning")) {
                        DisplayWarnings = GetTokenValue(info, tokens, true);
                    }
                }
                tokens.MoveToNextToken();
            }
        }

        private bool GetTokenValue(string info, TokenStream<RToken> tokens, bool defaultValue) {
            var t = tokens.MoveToNextToken();
            if (t.Length == 1 && info[t.Start] == '=') {
                t = tokens.MoveToNextToken();
                if (t.TokenType == RTokenType.Logical) {
                    var value = info.Substring(t.Start, t.Length);
                    if (value.EqualsOrdinal("FALSE") || value.EqualsOrdinal("F")) {
                        return false;
                    }
                    if (value.EqualsOrdinal("TRUE") || value.EqualsOrdinal("T")) {
                        return true;
                    }
                }
            }
            return defaultValue;
        }
    }
}
