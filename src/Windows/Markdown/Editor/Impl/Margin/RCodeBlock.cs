// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
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

        public async Task<string> EvaluateAsync(IRSession session, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            try {
                var result = await session.EvaluateAsync(Invariant($"evaluate::evaluate({_text})"), REvaluationKind.Normal, ct);
                Result = result.ToString();
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                Result = ex.Message;
            }
            State = CodeBlockEvalState.Evaluated;
            return Result;
        }
    }
}
