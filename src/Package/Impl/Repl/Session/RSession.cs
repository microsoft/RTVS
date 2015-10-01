using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Repl.Session
{
    internal sealed class RSession : IRSession, IRCallbacks
    {
        private readonly ConcurrentQueue<RSessionRequestSource> _pendingRequestSources = new ConcurrentQueue<RSessionRequestSource>();
        private readonly ConcurrentQueue<RSessionEvaluationSource> _pendingEvaluationSources = new ConcurrentQueue<RSessionEvaluationSource>();
        private readonly Stack<RSessionRequestSource> _currentRequestSources = new Stack<RSessionRequestSource>();

        public event EventHandler<RBeforeRequestEventArgs> BeforeRequest;
        public event EventHandler<RResponseEventArgs> Response;
        public event EventHandler<RErrorEventArgs> Error;
        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// ReadConsole requires a task even if there are no pending requests
        /// </summary>
        private TaskCompletionSource<string> _nextRequestTcs;
        private IReadOnlyCollection<IRContext> _contexts;
        private RHost _host;
        private Task _hostRunTask;
        private TaskCompletionSource<object> _initializationTcs;

        public string Prompt { get; private set; } = "> ";
        public int MaxLength { get; private set; } = 0x1000;
        public bool HostIsRunning => _hostRunTask != null && !_hostRunTask.IsCompleted;

        public void Dispose()
        {
            _host?.Dispose();
        }

        public Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true)
        {
            var requestTcs = GetRequestTcs();
            RSessionRequestSource requestSource;
            if (requestTcs == null)
            {
                requestSource = new RSessionRequestSource(isVisible, _contexts);
                _pendingRequestSources.Enqueue(requestSource);
            }
            else
            {
                requestSource = new RSessionRequestSource(isVisible, _contexts, requestTcs);
                requestSource.BeginInteractionAsync(Prompt, MaxLength);
                _currentRequestSources.Push(requestSource);
            }

            return requestSource.CreateRequestTask;
        }

        public Task<IRSessionEvaluation> BeginEvaluationAsync()
        {
            var source = new RSessionEvaluationSource();
            _pendingEvaluationSources.Enqueue(source);
            return source.Task;
        }

        public Task StartHostAsync()
        {
            if (_hostRunTask != null && !_hostRunTask.IsCompleted)
            {
                throw new InvalidOperationException("Another instance of RHost is running for this RSession. Stop it before starting new one.");
            }

            var psi = new ProcessStartInfo
            {
                WorkingDirectory = RToolsSettings.GetBinariesFolder(),
                UseShellExecute = false
            };

            psi.EnvironmentVariables["R_HOME"] = psi.WorkingDirectory.Substring(0, psi.WorkingDirectory.IndexOf(@"\bin\"));

            _host = new RHost(this);
            _initializationTcs = new TaskCompletionSource<object>();
            _hostRunTask = _host.CreateAndRun(psi);

            return Task.WhenAny(_initializationTcs.Task, _hostRunTask).Unwrap();
        }

        public async Task StopHostAsync()
        {
            if (_hostRunTask.IsCompleted)
            {
                return;
            }

            var request = await BeginInteractionAsync(false);
            if (_hostRunTask.IsCompleted)
            {
                request.Dispose();
                return;
            }

            await request.Quit();
            await _hostRunTask;
        }

        private TaskCompletionSource<string> GetRequestTcs()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                var requestTsc = Interlocked.Exchange(ref _nextRequestTcs, null);
                if (requestTsc != null)
                {
                    return requestTsc;
                }

                if (_pendingRequestSources.Count > 0)
                {
                    return null;
                }

                // If R host isn't connected yet, return null
                if (_contexts == null)
                {
                    return null;
                }

                // There is either another request that is created or ReadConsole hasn't yet created request tcs for empty queue
                spin.SpinOnce();
            }
        }

        Task IRCallbacks.Connected(string rVersion)
        {
            _initializationTcs.SetResult(null);
            return Task.CompletedTask;
        }

        Task IRCallbacks.Disconnected()
        {
            while (_currentRequestSources.Count > 0)
            {
                var requestSource = _currentRequestSources.Pop();
                requestSource.Complete();
            }
            
            _contexts = null;

            OnDisconnected();
            return Task.CompletedTask;
        }

        async Task<string> IRCallbacks.ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, string buf, int len, bool addToHistory)
        {
            foreach (var rsToCompleter in _currentRequestSources.PopWhile(rs => rs.Contexts.Count >= contexts.Count))
            {
                rsToCompleter.Complete();
            }

            _contexts = contexts;
            Prompt = prompt;
            MaxLength = len;

            OnBeforeRequest(contexts, prompt, len, addToHistory);

            while (true)
            {
                try
                {
                    string response = await ReadNextRequest(contexts, prompt, len);
                    Debug.Assert(response.Length < len); // len includes null terminator
                    if (response.Length >= len) {
                        response = response.Substring(0, len - 1);
                    }
                    return response;
                }
                catch (TaskCanceledException)
                {
                    //If request was cancelled, peek the next one
                }
            }
        }

        private Task<string> ReadNextRequest(IReadOnlyCollection<IRContext> contexts, string prompt, int len)
        {
            RSessionRequestSource requestSource;
            if (_pendingRequestSources.TryPeek(out requestSource) && requestSource.Contexts.SequenceEqual(contexts))
            {
                _pendingRequestSources.TryDequeue(out requestSource);
                _currentRequestSources.Push(requestSource);
                return requestSource.BeginInteractionAsync(prompt, len);
            }

            // If there are no pending requests, create tcs that will be used by the first newly added request
            _nextRequestTcs = new TaskCompletionSource<string>();
            return _nextRequestTcs.Task;
        }

        Task IRCallbacks.WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string buf, OutputType otype)
        {
            if (otype == OutputType.Error)
            {
                OnError(contexts, buf);
                int contextsCountAfterError = contexts.SkipWhile(c => c.CallFlag == RContextType.CCode).Count();

                foreach (var requestSource in _currentRequestSources.PopWhile(rs => rs.Contexts.Count >= contextsCountAfterError))
                {
                    requestSource.Fail(buf);
                }
            }
            else
            {
                OnResponse(contexts, buf);
            }

            foreach (var requestSource in _currentRequestSources)
            {
                requestSource.Write(buf);
            }

            return Task.CompletedTask;
        }

        async Task IRCallbacks.ShowMessage(IReadOnlyCollection<IRContext> contexts, string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);
            EditorShell.Current.ShowErrorMessage(message);
        }

        Task<YesNoCancel> IRCallbacks.YesNoCancel(IReadOnlyCollection<IRContext> contexts, string s)
        {
            return Task.FromResult(YesNoCancel.Yes);
        }

        Task IRCallbacks.Busy(IReadOnlyCollection<IRContext> contexts, bool which)
        {
            return Task.CompletedTask;
        }

        async Task IRCallbacks.Evaluate(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator)
        {
            RSessionEvaluationSource source;
            while (_pendingEvaluationSources.TryDequeue(out source))
            {
                await source.BeginEvaluationAsync(contexts, evaluator);
            }
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
                handlers(this, args);
            }
        }

        private void OnError(IReadOnlyCollection<IRContext> contexts, string message)
        {
            var handlers = Error;
            if (handlers != null && _currentRequestSources.All(rs => rs.IsVisible))
            {
                var args = new RErrorEventArgs(contexts, message);
                handlers(this, args);
            }
        }

        private void OnDisconnected()
        {
            var handlers = Disconnected;
            if (handlers != null)
            {
                var args = new EventArgs();
                handlers(this, args);
            }
        }
    }
}