// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client.Session {
    internal sealed class RSessionEvaluationSource {
        private readonly TaskCompletionSource<IRSessionEvaluation> _tcs;

        public bool IsMutating { get; }

        public RSessionEvaluationSource(bool isMutating, CancellationToken ct) {
            _tcs = new TaskCompletionSource<IRSessionEvaluation>();
            ct.Register(() => _tcs.TrySetCanceled(ct), false);
            IsMutating = isMutating;
        }

        public Task<IRSessionEvaluation> Task => _tcs.Task;

        public Task BeginEvaluationAsync(IReadOnlyList<IRContext> contexts, IRExpressionEvaluator evaluator, CancellationToken ct) {
            var evaluation = new RSessionEvaluation(contexts, evaluator, ct);
            return _tcs.TrySetResult(evaluation) ? evaluation.Task : System.Threading.Tasks.Task.CompletedTask;
        }

        public bool TryCancel() {
            return _tcs.TrySetCanceled();
        }
    }
}