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
        public int BlockNumber { get; }
        public string Text { get; }
        public int Hash { get; }
        public string Arguments { get; }
        public string Result { get; set; }
        public CodeBlockState State { get; set; } = CodeBlockState.Created;
        public bool Eval { get; private set; } = true;
        public bool DisplayErrors { get; private set; } = true;
        public bool DisplayWarnings { get; private set; } = true;
        public bool EchoContent { get; private set; } = true;

        public string HtmlElementId => Invariant($"rcode_{BlockNumber}_{Hash}");

        public RCodeBlock(int number, string text, string arguments = null) {
            BlockNumber = number;
            Text = text;
            Arguments = arguments ?? string.Empty;
            Hash = Text.GetHashCode() + Arguments.GetHashCode();
            ExtractOptions(arguments);
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
