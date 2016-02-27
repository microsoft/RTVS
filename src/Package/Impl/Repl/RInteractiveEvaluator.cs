using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Core.Parser;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal sealed class RInteractiveEvaluator : IInteractiveEvaluator {
        private readonly IRToolsSettings _toolsSettings;

        public IRHistory History { get; }
        public IRSession Session { get; }

        public RInteractiveEvaluator(IRSession session, IRHistory history, IRToolsSettings toolsSettings) {
            History = history;
            Session = session;
            Session.Output += SessionOnOutput;
            Session.Disconnected += SessionOnDisconnected;
            Session.BeforeRequest += SessionOnBeforeRequest;
            _toolsSettings = toolsSettings;
        }

        public void Dispose() {
            Session.Output -= SessionOnOutput;
            Session.Disconnected -= SessionOnDisconnected;
            Session.BeforeRequest -= SessionOnBeforeRequest;
        }

        public async Task<ExecutionResult> InitializeAsync() {
            try {
                if (!Session.IsHostRunning) {
                    await Session.StartHostAsync(new RHostStartupInfo {
                        Name = "REPL",
                        RBasePath = _toolsSettings.RBasePath,
                        RCommandLineArguments = _toolsSettings.RCommandLineArguments,
                        CranMirrorName = _toolsSettings.CranMirror,
                        WorkingDirectory = _toolsSettings.WorkingDirectory
                    });
                }
                return ExecutionResult.Success;
            } catch (RHostBinaryMissingException) {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);
                VsAppShell.Current.ShowErrorMessage(Resources.Error_Microsoft_R_Host_Missing);
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
            // TODO: Workaround for bug https://github.com/dotnet/roslyn/issues/8569
            if (text[0] == '\u0002') {
                text = text.Substring(1);
            }

            var start = 0;
            var end = text.IndexOf('\n');
            if (end == -1) {
                return ExecutionResult.Success;
            }

            try {
                Session.BeforeRequest -= SessionOnBeforeRequest;
                using (Session.DisableMutatedOnReadConsole()) {
                    while (end != -1) {
                        var line = text.Substring(start, end - start + 1);
                        start = end + 1;
                        end = text.IndexOf('\n', start);

                        using (var request = await Session.BeginInteractionAsync()) {
                            if (line.Length >= request.MaxLength) {
                                CurrentWindow.WriteErrorLine(string.Format(Resources.InputIsTooLong, request.MaxLength));
                                return ExecutionResult.Failure;
                            }

                            await request.RespondAsync(line);
                        }
                    }
                }

                return ExecutionResult.Success;
            } catch (RException) {
                // It was already reported via RSession.Error and printed out; just return failure.
                return ExecutionResult.Failure;
            } catch (OperationCanceledException) {
                // Cancellation reason was already reported via RSession.Error and printed out; just return failure.
                return ExecutionResult.Failure;
            } catch (Exception ex) {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);
                VsAppShell.Current.ShowErrorMessage(ex.ToString());
                return ExecutionResult.Failure;
            } finally {
                Session.BeforeRequest += SessionOnBeforeRequest;
                History.AddToHistory(text);
            }
        }

        public string FormatClipboard() {
            // keep the clipboard content as is
            return null;
        }

        public void AbortExecution() {
            Session.CancelAllAsync().DoNotWait();
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
                Write(args.Message.ToUnicodeQuotes());
            } else {
                WriteError(args.Message);
            }
        }

        private void SessionOnDisconnected(object sender, EventArgs args) {
            if (CurrentWindow == null || !CurrentWindow.IsResetting) {
                WriteLine(Resources.MicrosoftRHostStopped);
            }
        }

        private void SessionOnBeforeRequest(object sender, RRequestEventArgs e) {
            VsAppShell.Current.DispatchOnUIThread(() => {
                if (CurrentWindow == null || CurrentWindow.IsRunning) {
                    return;
                }

                var projectionBuffer = CurrentWindow.TextView.TextBuffer as IProjectionBuffer;
                if (projectionBuffer == null) {
                    return;
                }

                var spanCount = projectionBuffer.CurrentSnapshot.SpanCount;
                projectionBuffer.ReplaceSpans(spanCount - 2, 1, new List<object> {GetPrompt()}, EditOptions.None, new object());
            });
        }

        private void Write(string message) {
            if (CurrentWindow != null) {
                VsAppShell.Current.DispatchOnUIThread(() => CurrentWindow.Write(message));
            }
        }

        private void WriteError(string message) {
            if (CurrentWindow != null) {
                VsAppShell.Current.DispatchOnUIThread(() => CurrentWindow.WriteError(message));
            }
        }

        private void WriteLine(string message) {
            if (CurrentWindow != null) {
                VsAppShell.Current.DispatchOnUIThread(() => CurrentWindow.WriteLine(message));
            }
        }
    }
}