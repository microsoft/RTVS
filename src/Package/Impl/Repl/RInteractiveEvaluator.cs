using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

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
            _session.Disconnected += SessionOnDisconnected;
        }

        public void Dispose()
        {
            _session.BeforeRequest -= SessionOnBeforeRequest;
            _session.Response -= SessionOnResponse;
            _session.Error -= SessionOnError;
            _session.Disconnected -= SessionOnDisconnected;
        }

        public async Task<ExecutionResult> InitializeAsync()
        {
            try
            {
                await _session.StartHostAsync();
                return ExecutionResult.Success;
            }
            catch (MicrosoftRHostMissingException)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);
                IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                if (shell != null)
                {
                    int result;
                    shell.ShowMessageBox(0, Guid.Empty, null, Resources.Error_Microsoft_R_Host_Missing, null, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 0, out result);
                    Process.Start("http://www.microsoft.com");
                }
                return ExecutionResult.Failure;
            }
            catch (Exception)
            {
                return ExecutionResult.Failure;
            }
        }

        public async Task<ExecutionResult> ResetAsync(bool initialize = true)
        {
            CurrentWindow.WriteLine("Stoping R Host");
            await _session.StopHostAsync();

            if (initialize)
            {
                return await InitializeAsync();
            }

            return ExecutionResult.Success;
        }

        public bool CanExecuteCode(string text)
        {
            return true;
        }

        public async Task<ExecutionResult> ExecuteCodeAsync(string text)
        {
            _requestTcs = new TaskCompletionSource<ExecutionResult>();
            var request = await _session.BeginInteractionAsync();

            Task.Run(async () =>
            {
                try
                {
                    await request.RespondAsync(text);
                }
                catch (RException)
                {
                    // It was already reported via RSession.Error and printed out; do nothing.
                }
                catch (Exception ex)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);
                    IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                    if (shell != null)
                    {
                        int result;
                        shell.ShowMessageBox(0, Guid.Empty, null, ex.ToString(), null, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 0, out result);
                    }
                }
            }).DoNotWait();

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
            if (_requestTcs != null)
            {
                _requestTcs.SetResult(ExecutionResult.Success);
                _requestTcs = null;
            }
        }

        private void SessionOnResponse(object sender, RResponseEventArgs args)
        {
            CurrentWindow.Write(args.Message);
        }

        private void SessionOnError(object sender, RErrorEventArgs args)
        {
            CurrentWindow.WriteError(args.Message);
            if (_requestTcs != null)
            {
                _requestTcs.SetResult(ExecutionResult.Failure);
                _requestTcs = null;
            }
        }

        private void SessionOnDisconnected(object sender, EventArgs args)
        {
            CurrentWindow.WriteLine(Resources.MicrosoftRHostStopped);

            if (_requestTcs != null)
            {
                _requestTcs.SetResult(ExecutionResult.Success);
                _requestTcs = null;
            }
        }
    }
}