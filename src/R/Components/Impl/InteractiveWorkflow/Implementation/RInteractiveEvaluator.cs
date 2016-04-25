// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.History;
using Microsoft.R.Components.Settings;
using Microsoft.R.Core.Parser;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public sealed class RInteractiveEvaluator : IInteractiveEvaluator {
        private readonly ICoreShell _coreShell;
        private readonly IRSettings _settings;

        public IRHistory History { get; }
        public IRSession Session { get; }

        public RInteractiveEvaluator(IRSession session, IRHistory history, ICoreShell coreShell, IRSettings settings) {
            History = history;
            Session = session;
            Session.Output += SessionOnOutput;
            Session.Disconnected += SessionOnDisconnected;
            Session.BeforeRequest += SessionOnBeforeRequest;
            _coreShell = coreShell;
            _settings = settings;
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
                        RBasePath = _settings.RBasePath,
                        RHostCommandLineArguments = _settings.RCommandLineArguments,
                        CranMirrorName = _settings.CranMirror,
                        WorkingDirectory = _settings.WorkingDirectory
                    }, new RSessionCallback(CurrentWindow, Session, _settings, _coreShell));
                }
                return ExecutionResult.Success;
            } catch (RHostBinaryMissingException) {
                await _coreShell.DispatchOnMainThreadAsync(() => _coreShell.ShowErrorMessage(Resources.Error_Microsoft_R_Host_Missing));
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
            if (text.StartsWith("?", StringComparison.Ordinal)) {
                return true;
            }

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
            return true;
        }

        public async Task<ExecutionResult> ExecuteCodeAsync(string text) {
            var start = 0;
            var end = text.IndexOf('\n');
            if (end == -1) {
                return ExecutionResult.Success;
            }

            try {
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
            } catch (OperationCanceledException) {
                // Cancellation reason was already reported via RSession.Error and printed out; just return failure.
                return ExecutionResult.Failure;
            } catch (Exception ex) {
                await _coreShell.DispatchOnMainThreadAsync(() => _coreShell.ShowErrorMessage(ex.ToString()));
                return ExecutionResult.Failure;
            } finally {
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
            _coreShell.DispatchOnUIThread(() => {
                if (CurrentWindow == null || CurrentWindow.IsRunning) {
                    return;
                }

                var projectionBuffer = CurrentWindow.TextView.TextBuffer as IProjectionBuffer;
                if (projectionBuffer == null) {
                    return;
                }

                var spanCount = projectionBuffer.CurrentSnapshot.SpanCount;
                projectionBuffer.ReplaceSpans(spanCount - 2, 1, new List<object> { GetPrompt() }, EditOptions.None, new object());
            });
        }

        private void Write(string message) {
            if (CurrentWindow != null) {
                _coreShell.DispatchOnUIThread(() => CurrentWindow.Write(message));
            }
        }

        private void WriteError(string message) {
            if (CurrentWindow != null) {
                _coreShell.DispatchOnUIThread(() => CurrentWindow.WriteError(message));
            }
        }

        private void WriteLine(string message) {
            if (CurrentWindow != null) {
                _coreShell.DispatchOnUIThread(() => CurrentWindow.WriteLine(message));
            }
        }
    }
}