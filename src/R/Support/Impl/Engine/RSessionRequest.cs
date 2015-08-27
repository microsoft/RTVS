using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Microsoft.R.Support.Engine
{
	internal class RSessionRequest : IRSessionRequest
	{
		private readonly TaskCompletionSource<string> _requestTcs;
		private readonly TaskCompletionSource<string> _responseTcs;

		public string Prompt { get; }
		public int MaxLength { get; }
		public IReadOnlyCollection<IRContext> Contexts { get; }

		public RSessionRequest(TaskCompletionSource<string> requestTcs, TaskCompletionSource<string> responseTcs, string prompt, int maxLength, IReadOnlyCollection<IRContext> contexts)
		{
			_requestTcs = requestTcs;
			_responseTcs = responseTcs;
			Prompt = prompt;
			MaxLength = maxLength;
			Contexts = contexts;
		}

		public Task<string> Send(string messageText)
		{
			_requestTcs.SetResult(messageText);
			return _responseTcs.Task;
		}
	}
}