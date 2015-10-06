using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Repl.Session {
    internal sealed class RSession : IRSession, IRCallbacks {
        private readonly BufferBlock<RSessionRequestSource> _pendingRequestSources = new BufferBlock<RSessionRequestSource>();
        private readonly ConcurrentQueue<RSessionEvaluationSource> _pendingEvaluationSources = new ConcurrentQueue<RSessionEvaluationSource>();
        private readonly Stack<RSessionRequestSource> _currentRequestSources = new Stack<RSessionRequestSource>();

        public event EventHandler<RBeforeRequestEventArgs> BeforeRequest;
        public event EventHandler<RResponseEventArgs> Response;
        public event EventHandler<RErrorEventArgs> Error;
        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// ReadConsole requires a task even if there are no pending requests
        /// </summary>
        private IReadOnlyCollection<IRContext> _contexts;
        private RHost _host;
        private Task _hostRunTask;
        private TaskCompletionSource<object> _initializationTcs;

        public string Prompt { get; private set; } = "> ";
        public int MaxLength { get; private set; } = 0x1000;
        public bool HostIsRunning => _hostRunTask != null && !_hostRunTask.IsCompleted;

        public void Dispose() {
            _host?.Dispose();
        }

        public Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true) {
            RSessionRequestSource requestSource = new RSessionRequestSource(isVisible, _contexts);
            _pendingRequestSources.Post(requestSource);
            return requestSource.CreateRequestTask;
        }

        public Task<IRSessionEvaluation> BeginEvaluationAsync() {
            var source = new RSessionEvaluationSource();
            _pendingEvaluationSources.Enqueue(source);
            return source.Task;
        }

        public Task StartHostAsync() {
            if (_hostRunTask != null && !_hostRunTask.IsCompleted) {
                throw new InvalidOperationException("Another instance of RHost is running for this RSession. Stop it before starting new one.");
            }

            _host = new RHost(this);
            _initializationTcs = new TaskCompletionSource<object>();

            _hostRunTask = Task.Run(() => _host.CreateAndRun(RToolsSettings.GetRVersionPath()));

            var initializationTask = _initializationTcs.Task.ContinueWith(new Func<Task, Task>(AfterInitialization)).Unwrap();

            return Task.WhenAny(initializationTask, _hostRunTask).Unwrap();
        }

        public async Task StopHostAsync() {
            if (_hostRunTask.IsCompleted) {
                return;
            }

            var request = await BeginInteractionAsync(false);
            if (_hostRunTask.IsCompleted) {
                request.Dispose();
                return;
            }

            await request.Quit();
            await _hostRunTask;
        }

        private async Task AfterInitialization(Task task) {
            var interaction = await BeginInteractionAsync(false);
            await interaction.SetDefaultWorkingDirectory();
        }

        Task IRCallbacks.Connected(string rVersion) {
            _initializationTcs.SetResult(null);
            return Task.CompletedTask;
        }

        Task IRCallbacks.Disconnected() {
            while (_currentRequestSources.Count > 0) {
                var requestSource = _currentRequestSources.Pop();
                requestSource.Complete();
            }

            _contexts = null;

            OnDisconnected();
            return Task.CompletedTask;
        }

        async Task<string> IRCallbacks.ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, string buf, int len, bool addToHistory, CancellationToken ct) {
            foreach (var rsToCompleter in _currentRequestSources.PopWhile(rs => rs.Contexts.Count >= contexts.Count)) {
                rsToCompleter.Complete();
            }

            _contexts = contexts;
            Prompt = prompt;
            MaxLength = len;

            OnBeforeRequest(contexts, prompt, len, addToHistory);

            while (true) {
                ct.ThrowIfCancellationRequested();

                try {
                    return await ReadNextRequest(prompt, len, ct);
                } catch (TaskCanceledException) {
                    // If request was canceled through means other than our token, it indicates the refusal of
                    // that requestor to respond to that particular prompt, so move on to the next requestor.
                    // If it was canceled through the token, then host itself is shutting down, and cancellation
                    // will be propagated on the entry to next iteration of this loop.
                }
            }
        }

        private async Task<string> ReadNextRequest(string prompt, int len, CancellationToken ct) {
            var requestSource = await _pendingRequestSources.ReceiveAsync(ct);

            TaskCompletionSource<string> requestTcs = new TaskCompletionSource<string>();
            _currentRequestSources.Push(requestSource);
            requestSource.Request(prompt, len, requestTcs);

            // Cancel immediately if ct is canceled, even if request is not handled yet.
            // Disconnected handler will run next and take care of cleaning the queue.
            await Task.WhenAny(requestTcs.Task, Task.Delay(Timeout.Infinite, ct)).Unwrap();
            string response = await requestTcs.Task;

            Debug.Assert(response.Length < len); // len includes null terminator
            if (response.Length >= len) {
                response = response.Substring(0, len - 1);
            }

            return response;
        }

        Task IRCallbacks.WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string buf, OutputType otype, CancellationToken ct) {
            if (otype == OutputType.Error) {
                OnError(contexts, buf);
                int contextsCountAfterError = contexts.SkipWhile(c => c.CallFlag == RContextType.CCode).Count();

                foreach (var requestSource in _currentRequestSources.PopWhile(rs => rs.Contexts.Count >= contextsCountAfterError)) {
                    requestSource.Fail(buf);
                }
            } else {
                OnResponse(contexts, buf);
            }

            foreach (var requestSource in _currentRequestSources) {
                requestSource.Write(buf);
            }

            return Task.CompletedTask;
        }

        async Task IRCallbacks.ShowMessage(IReadOnlyCollection<IRContext> contexts, string message, CancellationToken ct) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);
            EditorShell.Current.ShowErrorMessage(message);
        }

        Task<YesNoCancel> IRCallbacks.YesNoCancel(IReadOnlyCollection<IRContext> contexts, string s, CancellationToken ct) {
            return Task.FromResult(YesNoCancel.Yes);
        }

        Task IRCallbacks.Busy(IReadOnlyCollection<IRContext> contexts, bool which, CancellationToken ct) {
            return Task.CompletedTask;
        }

        async Task IRCallbacks.Evaluate(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, CancellationToken ct) {
            RSessionEvaluationSource source;
            while (_pendingEvaluationSources.TryDequeue(out source)) {
                await source.BeginEvaluationAsync(contexts, evaluator, ct);
            }
        }

        private void OnBeforeRequest(IReadOnlyCollection<IRContext> contexts, string prompt, int maxLength, bool addToHistoty) {
            var handlers = BeforeRequest;
            if (handlers != null && _currentRequestSources.All(rs => rs.IsVisible)) {
                var args = new RBeforeRequestEventArgs(contexts, prompt, maxLength, addToHistoty);
                Task.Run(() => handlers(this, args));
            }
        }

        private void OnResponse(IReadOnlyCollection<IRContext> contexts, string message) {
            var handlers = Response;
            if (handlers != null && _currentRequestSources.All(rs => rs.IsVisible)) {
                var args = new RResponseEventArgs(contexts, message);
                handlers(this, args);
            }
        }

        private void OnError(IReadOnlyCollection<IRContext> contexts, string message) {
            var handlers = Error;
            if (handlers != null && _currentRequestSources.All(rs => rs.IsVisible)) {
                var args = new RErrorEventArgs(contexts, message);
                handlers(this, args);
            }
        }

        private void OnDisconnected() {
            var handlers = Disconnected;
            if (handlers != null) {
                var args = new EventArgs();
                handlers(this, args);
            }
        }
    }
}