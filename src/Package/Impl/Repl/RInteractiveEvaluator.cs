using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Repl
{
    internal sealed class RInteractiveEvaluator : IInteractiveEvaluator
    {
        private readonly IRSession _session;
        private TaskCompletionSource<ExecutionResult> _requestTcs;

        public RInteractiveEvaluator(IRSession session)
        {
            _session = session;
            _session.BeforeRequest += SessionOnBeforeRequest;
            _session.Response += SessionOnResponse;
            _session.Error += SessionOnError;
        }

        public void Dispose()
        {
            _session.BeforeRequest -= SessionOnBeforeRequest;
            _session.Response -= SessionOnResponse;
            _session.Error -= SessionOnError;
        }

        public async Task<ExecutionResult> InitializeAsync()
        {
            try
            {
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
                await _session.InitializeAsync();
                return ExecutionResult.Success;
            }
            catch (Exception)
            {
                return ExecutionResult.Failure;
            }
        }

        private async void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;

            if (e.Exception.InnerException is MicrosoftRHostMissingException)
            {
                e.SetObserved();
                IRCallbacks callbacks = _session as IRCallbacks;

                Debug.Assert(callbacks != null);
                if (callbacks != null)
                {
                    await callbacks.ShowMessage(new ReadOnlyCollection<IRContext>(new IRContext[0]), Resources.Error_Microsoft_R_Host_Missing);
                    // TODO: actually provide download link for Microsoft.R.Host.exe
                    Process.Start("http://www.microsoft.com");
                }
            }
        }

        public Task<ExecutionResult> ResetAsync(bool initialize = true)
        {
            throw new NotImplementedException();
        }

        public bool CanExecuteCode(string text)
        {
            return true;
        }

        public async Task<ExecutionResult> ExecuteCodeAsync(string text)
        {
            _requestTcs = new TaskCompletionSource<ExecutionResult>();

            var request = await _session.BeginInteractionAsync();
            request.RespondAsync(text).DoNotWait(); // TODO: Add logging for unexpected exceptions (exception from R host will be handled in SessionOnError)
            return await _requestTcs.Task;
        }

        public string FormatClipboard()
        {
            // keep the clipboard content as is
            return null;
        }

        public void AbortExecution()
        {
            //TODO: Find out if we can cancel long executions in R. For now - do nothing.
        }

        public string GetPrompt()
        {
            return _session.Prompt;
        }

        public IInteractiveWindow CurrentWindow { get; set; }

        private void SessionOnBeforeRequest(object sender, RBeforeRequestEventArgs args)
        {
            Debug.Assert(_requestTcs != null);
            if (_requestTcs != null)
            {
                _requestTcs.SetResult(ExecutionResult.Success);
            }
        }

        private void SessionOnResponse(object sender, RResponseEventArgs args)
        {
            CurrentWindow.Write(args.Message);
        }

        private void SessionOnError(object sender, RErrorEventArgs args)
        {
            CurrentWindow.WriteError(args.Message);

            Debug.Assert(_requestTcs != null);
            if (_requestTcs != null)
            {
                _requestTcs.SetResult(ExecutionResult.Failure);
            }
        }
    }
}