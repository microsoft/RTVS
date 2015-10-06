using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl.Session
{
    internal sealed class RSessionRequestSource
    {
        private readonly TaskCompletionSource<IRSessionInteraction> _createRequestTcs;
        private readonly TaskCompletionSource<string> _responseTcs;
        private StringBuilder _sb;

        public Task<IRSessionInteraction> CreateRequestTask => _createRequestTcs.Task;
        public bool IsVisible { get; }
        public IReadOnlyCollection<IRContext> Contexts { get; }

        public RSessionRequestSource(bool isVisible, IReadOnlyCollection<IRContext> contexts)
        {
            _createRequestTcs = new TaskCompletionSource<IRSessionInteraction>();
            _responseTcs = new TaskCompletionSource<string>();

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
            Write(text);
            _responseTcs.SetException(new RException(_sb.ToString()));
        }

        public void Complete()
        {
            _responseTcs.SetResult(_sb?.ToString());
        }

        public bool TryCancel() {
            return _responseTcs.TrySetCanceled();
        }

        public void Write(string text)
        {
            if (_sb == null)
            {
                _sb = new StringBuilder();
            }
            _sb.Append(text);
        }
    }
}