using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Support.Engine
{
	internal class RSession : IRSession, IREndpoint
	{
		private readonly ConcurrentQueue<RSessionRequestSource> _pendingRequestSources = new ConcurrentQueue<RSessionRequestSource>();
		private readonly Stack<RSessionRequestSource> _currentRequestSources = new Stack<RSessionRequestSource>();

		public event EventHandler<RBeforeRequestEventArgs> BeforeRequest;
		public event EventHandler<RResponseEventArgs> Response;
		public event EventHandler<RErrorEventArgs> Error;

		/// <summary>
		/// ReadConsole requires a task even if there are no pending requests
		/// </summary>
		private TaskCompletionSource<string> _nextRequestTcs;
		private IReadOnlyCollection<IRContext> _contexts;


		public Task<IRSessionRequest> CreateRequest(bool isVisible = true, bool isContextBound = false)
		{
			var requestTcs = GetRequestTcs();
			var requestSource = new RSessionRequestSource(isVisible, _contexts, requestTcs);
			_pendingRequestSources.Enqueue(requestSource);

			return requestSource.CreateRequestTask;
		}

		private TaskCompletionSource<string> GetRequestTcs()
		{
			SpinWait spin = new SpinWait();
			while (true)
			{
				var responceTsc = Interlocked.Exchange(ref _nextRequestTcs, null);
				if (responceTsc != null)
				{
					return responceTsc;
				}

				if (_pendingRequestSources.Count > 0)
				{
					return new TaskCompletionSource<string>();
				}

				// There is either another request that is created or ReadConsole hasn't yet created request tcs for empty queue
				spin.SpinOnce();
			}
		}

		public Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, int maxLength, bool addToHistoty)
		{
			RSessionRequestSource requestSource;
			while (_currentRequestSources.Count > 0)
			{
				requestSource = _currentRequestSources.Peek();
				if (requestSource.Contexts.Count < contexts.Count)
				{
					break;
				}

				_currentRequestSources.Pop();
				requestSource.Complete();
			}

			_contexts = contexts;
			OnBeforeRequest(contexts, prompt, maxLength, addToHistoty);

			if (_pendingRequestSources.TryPeek(out requestSource) && requestSource.Contexts.SequenceEqual(contexts))
			{
				_pendingRequestSources.TryDequeue(out requestSource);
				_currentRequestSources.Push(requestSource);

				return requestSource.CreateRequest(prompt, maxLength);
			}

			// If there are no pending requests, create tcs that will be used by the first newly added request
			_nextRequestTcs = new TaskCompletionSource<string>();
			return _nextRequestTcs.Task;
		}

		public Task WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string message, bool isError)
		{
			if (isError)
			{
				OnError(contexts, message);
				int contextsCountAfterError = contexts.SkipWhile(c => c.CallFlag == RContextType.CCode).Count();

				while (_currentRequestSources.Count > 0)
				{
					var requestSource = _currentRequestSources.Peek();
					if (requestSource.Contexts.Count < contextsCountAfterError)
					{
						break;
					}

					_currentRequestSources.Pop();
					requestSource.Fail(message);
				}
			}
			else
			{
				OnResponse(contexts, message);
			}

			foreach (var requestSource in _currentRequestSources)
			{
				requestSource.Write(message);
			}

			return Task.CompletedTask;
		}

		private void OnBeforeRequest(IReadOnlyCollection<IRContext> contexts, string prompt, int maxLength, bool addToHistoty)
		{
			var handlers = BeforeRequest;
			if (handlers != null && _currentRequestSources.All(rs => rs.IsVisible))
			{
				var args = new RBeforeRequestEventArgs(contexts, prompt, maxLength, addToHistoty);
				Task.Run(() => handlers(this, args));
			}
		}

		private void OnResponse(IReadOnlyCollection<IRContext> contexts, string message)
		{
			var handlers = Response;
			if (handlers != null && _currentRequestSources.All(rs => rs.IsVisible))
			{
				var args = new RResponseEventArgs(contexts, message);
				Task.Run(() => handlers(this, args));
			}
		}

		private void OnError(IReadOnlyCollection<IRContext> contexts, string message)
		{
			var handlers = Error;
			if (handlers != null && _currentRequestSources.All(rs => rs.IsVisible))
			{
				var args = new RErrorEventArgs(contexts, message);
				Task.Run(() => handlers(this, args));
			}
		}
	}
}