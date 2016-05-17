// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Session {
    internal sealed class RSessionRequestSource {
        private readonly TaskCompletionSource<IRSessionInteraction> _createRequestTcs;
        private readonly TaskCompletionSource<object> _responseTcs;

        public Task<IRSessionInteraction> CreateRequestTask => _createRequestTcs.Task;
        public bool IsVisible { get; }

        public RSessionRequestSource(bool isVisible, CancellationToken ct) {
            _createRequestTcs = new TaskCompletionSource<IRSessionInteraction>();
            _responseTcs = new TaskCompletionSource<object>();
            ct.Register(() => _createRequestTcs.TrySetCanceled(ct));

            IsVisible = isVisible;
        }

        public void Request(IReadOnlyList<IRContext> contexts, string prompt, int maxLength, TaskCompletionSource<string> requestTcs) {
            var request = new RSessionInteraction(requestTcs, _responseTcs, prompt, maxLength, contexts ?? new[] { RHost.TopLevelContext });
            if (_createRequestTcs.TrySetResult(request)) {
                return;
            }

            request.Dispose();
            if (CreateRequestTask.IsCanceled) {
                throw new OperationCanceledException();
            }
        }

        public void CompleteResponse() {
            _responseTcs.SetResult(null);
        }

        public void Cancel() {
            _createRequestTcs.TrySetCanceled();
            _responseTcs.TrySetCanceled();
        }
    }
}