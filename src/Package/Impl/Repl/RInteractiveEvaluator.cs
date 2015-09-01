using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;

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
                await _session.InitializeAsync();
                return ExecutionResult.Success;
            }
            catch (Exception)
            {
                return ExecutionResult.Failure;
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
            _requestTcs.SetResult(ExecutionResult.Success);
        }

        private void SessionOnResponse(object sender, RResponseEventArgs args)
        {
            CurrentWindow.Write(args.Message);
        }

        private void SessionOnError(object sender, RErrorEventArgs args)
        {
            CurrentWindow.WriteError(args.Message);
            _requestTcs.SetResult(ExecutionResult.Failure);
        }
    }
}