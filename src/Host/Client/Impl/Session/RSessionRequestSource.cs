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
        public IReadOnlyList<IRContext> Contexts { get; }

        public RSessionRequestSource(bool isVisible, IReadOnlyList<IRContext> contexts, CancellationToken ct) {
            _createRequestTcs = new TaskCompletionSource<IRSessionInteraction>();
            _responseTcs = new TaskCompletionSource<object>();
            ct.Register(() => _createRequestTcs.TrySetCanceled(ct));

            IsVisible = isVisible;
            Contexts = contexts ?? new[] { RHost.TopLevelContext };
        }

        public void Request(string prompt, int maxLength, TaskCompletionSource<string> requestTcs) {
            var request = new RSessionInteraction(requestTcs, _responseTcs, prompt, maxLength, Contexts);
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