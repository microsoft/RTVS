using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl.Session
{
    internal class RSessionRequestSource
    {
        private readonly TaskCompletionSource<string> _requestTcs;
        private readonly TaskCompletionSource<IRSessionInteraction> _createRequestTcs;
        private readonly TaskCompletionSource<string> _responseTcs;
        private StringBuilder _sb;

        public Task<IRSessionInteraction> CreateRequestTask => _createRequestTcs.Task;
        public bool IsVisible { get; }
        public IReadOnlyCollection<IRContext> Contexts { get; }

        public RSessionRequestSource(bool isVisible, IReadOnlyCollection<IRContext> contexts, TaskCompletionSource<string> requestTcs = null)
        {
            _createRequestTcs = new TaskCompletionSource<IRSessionInteraction>();
            _requestTcs = requestTcs ?? new TaskCompletionSource<string>();
            _responseTcs = new TaskCompletionSource<string>();

            IsVisible = isVisible;
            Contexts = contexts;
        }

        public Task<string> BeginInteractionAsync(string prompt, int maxLength)
        {
            var request = new RSessionInteraction(_requestTcs, _responseTcs, prompt, maxLength, Contexts);
            _createRequestTcs.SetResult(request);
            return _requestTcs.Task;
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