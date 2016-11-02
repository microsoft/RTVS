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
        private CancellationTokenRegistration _clientCancellationRegistration;

        public RSessionEvaluationSource(CancellationToken clientCancellationToken) {
            _tcs = new TaskCompletionSourceEx<IRSessionEvaluation>();
            _clientCancellationRegistration = clientCancellationToken.Register(() => _tcs.TrySetCanceled(cancellationToken: clientCancellationToken));
        }

        public Task<IRSessionEvaluation> Task => _tcs.Task;

        public async Task<bool> BeginEvaluationAsync(IReadOnlyList<IRContext> contexts, IRExpressionEvaluator evaluator, CancellationToken hostCancellationToken) {
            var evaluation = new RSessionEvaluation(contexts, evaluator, hostCancellationToken);
            if (_tcs.TrySetResult(evaluation)) {
                _clientCancellationRegistration.Dispose();
                await evaluation.Task;
            }
            return evaluation.IsMutating;
        }

        public bool TryCancel(OperationCanceledException exception) {
            _clientCancellationRegistration.Dispose();
            return _tcs.TrySetCanceled(exception);
        }
    }
}