// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Tasks;

namespace Microsoft.R.Host.Client.Session {
    internal sealed class RSessionRequestSource {
        private readonly TaskCompletionSourceEx<IRSessionInteraction> _createRequestTcs;
        private readonly TaskCompletionSourceEx<object> _responseTcs;
        private CancellationTokenRegistration _cancellationTokenRegistration;

        public Task<IRSessionInteraction> CreateRequestTask => _createRequestTcs.Task;
        public bool IsVisible { get; }

        public RSessionRequestSource(bool isVisible, CancellationToken ct) {
            _createRequestTcs = new TaskCompletionSourceEx<IRSessionInteraction>();
            _responseTcs = new TaskCompletionSourceEx<object>();
            _cancellationTokenRegistration = ct.Register(() => _createRequestTcs.TrySetCanceled(cancellationToken:ct));

            IsVisible = isVisible;
        }

        public void Request(IReadOnlyList<IRContext> contexts, string prompt, int maxLength, TaskCompletionSource<string> requestTcs) {
            var request = new RSessionInteraction(requestTcs, _responseTcs.Task, prompt, maxLength, contexts ?? new[] { RHost.TopLevelContext });
            if (_createRequestTcs.TrySetResult(request)) {
                _cancellationTokenRegistration.Dispose();
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

        public void TryCancel(OperationCanceledException exception) {
            _createRequestTcs.TrySetCanceled(exception);
            _responseTcs.TrySetCanceled(exception);
            _cancellationTokenRegistration.Dispose();
        }
    }
}