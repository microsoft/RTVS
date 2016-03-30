// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Session {
    internal sealed class RSessionEvaluation : IRSessionEvaluation {
        private readonly IRExpressionEvaluator _evaluator;
        private readonly TaskCompletionSource<object> _tcs;
        private readonly CancellationToken _ct;

        public IReadOnlyList<IRContext> Contexts { get; }
        public bool IsMutating { get; }
        public Task Task => _tcs.Task;

        public RSessionEvaluation(IReadOnlyList<IRContext> contexts, IRExpressionEvaluator evaluator, bool isMutating, CancellationToken ct) {
            Contexts = contexts;
            _evaluator = evaluator;
            IsMutating = isMutating;
            _tcs = new TaskCompletionSource<object>();
            _ct = ct;
            ct.Register(() => _tcs.TrySetCanceled());
        }

        public void Dispose() {
            _tcs.TrySetResult(null);
        }

        public Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken ct) {
            if (IsMutating) {
                kind |= REvaluationKind.Mutating;
            }
            ct.Register(() => _tcs.TrySetCanceled());
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_ct, ct);
            return _evaluator.EvaluateAsync(expression, kind, cts.Token);
        }
    }
}