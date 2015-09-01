using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client
{
    internal class RSessionInteraction : IRSessionInteraction
    {
        private readonly TaskCompletionSource<string> _requestTcs;
        private readonly TaskCompletionSource<string> _responseTcs;

        public string Prompt { get; }
        public int MaxLength { get; }
        public IReadOnlyCollection<IRContext> Contexts { get; }

        public RSessionInteraction(TaskCompletionSource<string> requestTcs, TaskCompletionSource<string> responseTcs, string prompt, int maxLength, IReadOnlyCollection<IRContext> contexts)
        {
            _requestTcs = requestTcs;
            _responseTcs = responseTcs;
            Prompt = prompt;
            MaxLength = maxLength;
            Contexts = contexts;
        }

        public Task<string> RespondAsync(string messageText)
        {
            _requestTcs.SetResult(messageText);
            return _responseTcs.Task;
        }
    }
}