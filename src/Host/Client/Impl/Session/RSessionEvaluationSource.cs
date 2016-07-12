// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Tasks;

namespace Microsoft.R.Host.Client.Session {
    internal sealed class RSessionEvaluationSource {
        private readonly TaskCompletionSourceEx<IRSessionEvaluation> _tcs;

        public RSessionEvaluationSource(CancellationToken ct) {
            _tcs = new TaskCompletionSourceEx<IRSessionEvaluation>();
            ct.Register(() => _tcs.TrySetCanceled(cancellationToken: ct), false);
        }

        public Task<IRSessionEvaluation> Task => _tcs.Task;

        public async Task<bool> BeginEvaluationAsync(IReadOnlyList<IRContext> contexts, IRExpressionEvaluator evaluator, CancellationToken ct) {
            var evaluation = new RSessionEvaluation(contexts, evaluator, ct);
            if (_tcs.TrySetResult(evaluation)) {
                await evaluation.Task;
            }
            return evaluation.IsMutating;
        }

        public bool TryCancel(OperationCanceledException exception) {
            return _tcs.TrySetCanceled(exception);
        }
    }
}