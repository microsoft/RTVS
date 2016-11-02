// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Tasks;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Host.Client.Session {
    internal sealed class RSessionEvaluation : IRSessionEvaluation {
        private readonly IRExpressionEvaluator _evaluator;
        private readonly TaskCompletionSourceEx<object> _tcs;
        private CancellationTokenRegistration _hostCancellationRegistration;
        private CancellationTokenRegistration _clientCancellationRegistration;

        public IReadOnlyList<IRContext> Contexts { get; }
        public bool IsMutating { get; private set; }
        public Task Task => _tcs.Task;

        public RSessionEvaluation(IReadOnlyList<IRContext> contexts, IRExpressionEvaluator evaluator, CancellationToken hostCancellationToken) {
            Contexts = contexts;
            _evaluator = evaluator;
            _tcs = new TaskCompletionSourceEx<object>();
            _hostCancellationRegistration = hostCancellationToken.Register(() => _tcs.TrySetCanceled(new RHostDisconnectedException()));
        }

        public void Dispose() {
            _hostCancellationRegistration.Dispose();
            _clientCancellationRegistration.Dispose();
            _tcs.TrySetResult(null);
        }

        public Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken ct) {
            if (kind.HasFlag(REvaluationKind.Mutating)) {
                IsMutating = true;
            }

            _clientCancellationRegistration = ct.Register(() => _tcs.TrySetCanceled());
            return _evaluator.EvaluateAsync(expression, kind, ct);
        }
    }
}