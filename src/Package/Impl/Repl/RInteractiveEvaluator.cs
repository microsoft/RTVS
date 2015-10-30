using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Core.Parser;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal sealed class RInteractiveEvaluator : IInteractiveEvaluator {
        public IRSession Session { get; private set; }

        public RInteractiveEvaluator(IRSession session) {
            Session = session;
            Session.Output += SessionOnOutput;
            Session.Disconnected += SessionOnDisconnected;
        }

        public void Dispose() {
            Session.Output -= SessionOnOutput;
            Session.Disconnected -= SessionOnDisconnected;
        }

        public async Task<ExecutionResult> InitializeAsync() {
            try {
                await Session.StartHostAsync();
                return ExecutionResult.Success;
            } catch (MicrosoftRHostMissingException) {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);
                EditorShell.Current.ShowErrorMessage(Resources.Error_Microsoft_R_Host_Missing);
                return ExecutionResult.Failure;
            } catch (Exception) {
                return ExecutionResult.Failure;
            }
        }

        public async Task<ExecutionResult> ResetAsync(bool initialize = true) {
            try {
                if (Session.IsHostRunning) {
                    CurrentWindow.WriteError(Resources.MicrosoftRHostStopping + Environment.NewLine);
                    await Session.StopHostAsync();
                }

                if (!initialize) {
                    return ExecutionResult.Success;
                }

                CurrentWindow.WriteError(Resources.MicrosoftRHostStarting + Environment.NewLine);
                return await InitializeAsync();
            } catch (Exception ex) {
                Trace.Fail($"Exception in RInteractiveEvaluator.ResetAsync\n{ex}");
                return ExecutionResult.Failure;
            }
        }

        public bool CanExecuteCode(string text) {
            if (!text.StartsWith("?", StringComparison.Ordinal)) {
                var ast = RParser.Parse(text);
                if (ast.Errors.Count > 0) {
                    // if we have any errors other than an incomplete statement send the
                    // bad code to R.  Otherwise continue reading input.
                    foreach (var error in ast.Errors) {
                        if (error.ErrorType != ParseErrorType.CloseCurlyBraceExpected &&
                            error.ErrorType != ParseErrorType.CloseBraceExpected &&
                            error.ErrorType != ParseErrorType.CloseSquareBracketExpected &&
                            error.ErrorType != ParseErrorType.FunctionBodyExpected &&
                            error.ErrorType != ParseErrorType.RightOperandExpected) {
                            return true;
                        }
                    }

                    return false;
                }
            }
            return true;
        }

        public async Task<ExecutionResult> ExecuteCodeAsync(string text) {
            var request = await Session.BeginInteractionAsync();

            if (text.Length >= request.MaxLength) {
                CurrentWindow.WriteErrorLine(string.Format(Resources.InputIsTooLong, request.MaxLength));
                request.Dispose();
                return ExecutionResult.Failure;
            }

            try {
                await request.RespondAsync(text);
                return ExecutionResult.Success;
            } catch (RException) {
                // It was already reported via RSession.Error and printed out; just return failure.
                return ExecutionResult.Failure;
            } catch (OperationCanceledException) {
                // Cancellation reason was already reported via RSession.Error and printed out; just return failure.
                return ExecutionResult.Failure;
            } catch (Exception ex) {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);
                EditorShell.Current.ShowErrorMessage(ex.ToString());
                return ExecutionResult.Failure;
            }
        }

        public string FormatClipboard() {
            // keep the clipboard content as is
            return null;
        }

        public void AbortExecution() {
            Session.CancelAllAsync().SilenceException<OperationCanceledException>().DoNotWait();
        }

        public string GetPrompt() {
            if (CurrentWindow.CurrentLanguageBuffer.CurrentSnapshot.LineCount > 1) {
                // TODO: We should support dynamically getting the prompt at runtime
                // if the user changes it
                return "+ ";
            }
            return Session.Prompt;
        }

        public IInteractiveWindow CurrentWindow { get; set; }

        private void SessionOnOutput(object sender, ROutputEventArgs args) {
            if (args.OutputType == OutputType.Output) {
                Write(args.Message).DoNotWait();
            } else {
                WriteError(args.Message).DoNotWait();
            }
        }

        private void SessionOnDisconnected(object sender, EventArgs args) {
            if (!CurrentWindow.IsResetting) {
                WriteLine(Resources.MicrosoftRHostStopped).DoNotWait();
            }
        }

        private async Task Write(string message) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            CurrentWindow.Write(message);
        }

        private async Task WriteError(string message) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            CurrentWindow.WriteError(message);
        }

        private async Task WriteLine(string message) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            CurrentWindow.WriteLine(message);
        }
    }
}