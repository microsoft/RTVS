using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Support.Engine
{
	internal class RSessionRequestSource
	{
		private readonly TaskCompletionSource<string> _requestTcs;
		private readonly TaskCompletionSource<IRSessionRequest> _createRequestTcs;
		private readonly TaskCompletionSource<string> _responseTcs;
		private StringBuilder _sb;

		public Task<IRSessionRequest> CreateRequestTask => _createRequestTcs.Task;
		public bool IsVisible { get; }
		public IReadOnlyCollection<IRContext> Contexts { get; }

		public RSessionRequestSource(bool isVisible, IReadOnlyCollection<IRContext> contexts, TaskCompletionSource<string> requestTcs)
		{
			_createRequestTcs = new TaskCompletionSource<IRSessionRequest>();
			_requestTcs = requestTcs;
			_responseTcs = new TaskCompletionSource<string>();

			IsVisible = isVisible;
			Contexts = contexts;
		}

		public Task<string> CreateRequest(string prompt, int maxLength)
		{
			var request = new RSessionRequest(_requestTcs, _responseTcs, prompt, maxLength, Contexts);
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