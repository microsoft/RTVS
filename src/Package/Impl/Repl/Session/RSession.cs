using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.R.Package.Repl.Session
{
    using Task = System.Threading.Tasks.Task;

    internal sealed class RSession : IRSession, IRCallbacks
    {
        private readonly RHost _host;
        private readonly TaskCompletionSource<object> _initializationTcs;
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

        public string Prompt { get; private set; } = "> ";
        public int MaxLength { get; private set; } = 0x1000;

        public RSession()
        {
            _host = new RHost(this);
            _initializationTcs = new TaskCompletionSource<object>();
        }

        public void Dispose()
        {
            _host.Dispose();
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

        public Task InitializeAsync()
        {
            return Task.WhenAny(_initializationTcs.Task, _host.CreateAndRun());
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

                // There is either another request that is created or ReadConsole hasn't yet created request tcs for empty queue
                spin.SpinOnce();
            }
        }

        public Task Connected(string rVersion)
        {
            _initializationTcs.SetResult(null);
            return Task.CompletedTask;
        }

        public Task Disconnected()
        {
            return Task.CompletedTask;
        }

        public Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, string buf, int len, bool addToHistory)
        {
            foreach (var rsToCompleter in _currentRequestSources.PopWhile(rs => rs.Contexts.Count >= contexts.Count))
            {
                rsToCompleter.Complete();
            }

            _contexts = contexts;
            Prompt = prompt;
            MaxLength = len;

            OnBeforeRequest(contexts, prompt, len, addToHistory);

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

        public Task WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string buf, OutputType otype)
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

        public async Task ShowMessage(IReadOnlyCollection<IRContext> contexts, string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);

            IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            if (shell != null)
            {
                int result;
                shell.ShowMessageBox(0, Guid.Empty, null, message, null, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 0, out result);
            }
        }

        public Task<YesNoCancel> YesNoCancel(IReadOnlyCollection<IRContext> contexts, string s)
        {
            return Task.FromResult(Microsoft.R.Host.Client.YesNoCancel.Yes);
        }

        public Task Busy(IReadOnlyCollection<IRContext> contexts, bool which)
        {
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