using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl.Session
{
    internal sealed class RSessionRequestSource
    {
        private readonly TaskCompletionSource<IRSessionInteraction> _createRequestTcs;
        private readonly TaskCompletionSource<object> _responseTcs;

        public Task<IRSessionInteraction> CreateRequestTask => _createRequestTcs.Task;
        public bool IsVisible { get; }
        public IReadOnlyList<IRContext> Contexts { get; }

        public RSessionRequestSource(bool isVisible, IReadOnlyList<IRContext> contexts)
        {
            _createRequestTcs = new TaskCompletionSource<IRSessionInteraction>();
            _responseTcs = new TaskCompletionSource<object>();

            IsVisible = isVisible;
            Contexts = contexts ?? new[] { RHost.TopLevelContext };
        }

        public void Request(string prompt, int maxLength, TaskCompletionSource<string> requestTcs)
        {
            var request = new RSessionInteraction(requestTcs, _responseTcs, prompt, maxLength, Contexts);
            _createRequestTcs.SetResult(request);
        }

        public void Fail(string text)
        {
            _responseTcs.SetException(new RException(text));
        }

        public void Complete()
        {
            _responseTcs.SetResult(null);
        }

        public bool TryCancel() {
            return _responseTcs.TrySetCanceled();
        }
    }
}