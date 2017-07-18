// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.History;
using Microsoft.R.Components.Settings;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public sealed class RInteractiveEvaluator : IInteractiveEvaluator {
        private readonly DisposableBag _disposableBag = DisposableBag.Create<RInteractiveEvaluator>();
        private readonly IRSessionProvider _sessionProvider;
        private readonly IConnectionManager _connections;
        private readonly ICoreShell _coreShell;
        private readonly IRSettings _settings;
        private readonly IConsole _console;
        private readonly CountdownDisposable _evaluatorRequest;
        private readonly IFileSystem _fs;
        private InteractiveWindowWriter _windowWriter;
        private int _terminalWidth = 80;
        private IInteractiveWindow _currentWindow;
        private bool _brokerChanging;

        public IRHistory History { get; }
        public IRSession Session { get; }

        public RInteractiveEvaluator(IRSessionProvider sessionProvider, IRSession session, IRHistory history, IConnectionManager connections, ICoreShell coreShell, IRSettings settings, IConsole console) {
            History = history;
            Session = session;

            _sessionProvider = sessionProvider;
            _connections = connections;
            _coreShell = coreShell;
            _settings = settings;
            _console = console;
            _evaluatorRequest = new CountdownDisposable();
            _fs = _coreShell.FileSystem();

            _disposableBag
                .Add(() => Session.Output -= SessionOnOutput)
                .Add(() => Session.Connected -= SessionOnConnected)
                .Add(() => Session.Disconnected -= SessionOnDisconnected)
                .Add(() => Session.BeforeRequest -= SessionOnBeforeRequest)
                .Add(() => Session.AfterRequest -= SessionOnAfterRequest)
                .Add(() => _sessionProvider.BrokerChanged -= OnBrokerChanging);

            _sessionProvider.BrokerChanging += OnBrokerChanging;

            Session.Output += SessionOnOutput;
            Session.Connected += SessionOnConnected;
            Session.Disconnected += SessionOnDisconnected;
            Session.BeforeRequest += SessionOnBeforeRequest;
            Session.AfterRequest += SessionOnAfterRequest;
        }

        private void OnBrokerChanging(object sender, EventArgs e) => _brokerChanging = true;

        public void Dispose() {
            _disposableBag.TryDispose();
            _windowWriter?.Dispose();
            if (CurrentWindow != null) {
                CurrentWindow.TextView.VisualElement.SizeChanged -= VisualElement_SizeChanged;
            }
        }

        public Task<ExecutionResult> InitializeAsync() => InitializeAsync(false);

        private async Task<ExecutionResult> InitializeAsync(bool isResetting) {
            try {
                if (!Session.IsHostRunning) {
                    var startupInfo = new RHostStartupInfo(_settings.CranMirror, _settings.WorkingDirectory, _settings.RCodePage, _terminalWidth, !isResetting, true, true);
                    await Session.EnsureHostStartedAsync(startupInfo, new RSessionCallback(CurrentWindow, Session, _settings, _coreShell, _fs));
                }
                return ExecutionResult.Success;
            } catch (ComponentBinaryMissingException cbmex) {
                await _coreShell.ShowErrorMessageAsync(cbmex.Message);
                return ExecutionResult.Failure;
            } catch (RHostDisconnectedException ex) {
                WriteRHostDisconnectedError(ex);
                return ExecutionResult.Success;
            } catch (Exception ex) {
                await _coreShell.ShowErrorMessageAsync(ex.Message);
                return ExecutionResult.Failure;
            }
        }

        public async Task<ExecutionResult> ResetAsync(bool initialize = true) {
            try {
                if (Session.IsHostRunning) {
                    await SaveStateAsync();
                    WriteErrorLine(Environment.NewLine + Resources.MicrosoftRHostStopping);
                    await Session.StopHostAsync(true);
                }

                if (!initialize) {
                    return ExecutionResult.Success;
                }

                WriteErrorLine(Environment.NewLine + Resources.MicrosoftRHostStarting);
                return await InitializeAsync(isResetting: true);
            } catch (Exception ex) {
                Trace.Fail($"Exception in RInteractiveEvaluator.ResetAsync\n{ex}");
                return ExecutionResult.Failure;
            }
        }

        private async Task SaveStateAsync() {
            try {
                if (_settings.ShowSaveOnResetConfirmationDialog == YesNo.Yes) {
                    if (MessageButtons.Yes == await _coreShell.ShowMessageAsync(Resources.Warning_SaveOnReset, MessageButtons.YesNo)) {
                        await Session.ExecuteAsync("rtvs:::save_state()");
                    }
                }
            } catch (RHostDisconnectedException rhdex) {
                WriteRHostDisconnectedError(rhdex);
                WriteErrorLine(Resources.Error_FailedToSaveState);
            }
        }

        public bool CanExecuteCode(string text) {
            if (text.StartsWith("?", StringComparison.Ordinal)) {
                return true;
            }

            // if we have any errors other than an incomplete statement send the
            // bad code to R.  Otherwise continue reading input.
            var ast = RParser.Parse(text);
            return ast.IsCompleteExpression();
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
                            using (_evaluatorRequest.Increment()) {
                                if (line.Length >= request.MaxLength) {
                                    CurrentWindow.WriteErrorLine(string.Format(Resources.InputIsTooLong, request.MaxLength));
                                    return ExecutionResult.Failure;
                                }

                                await request.RespondAsync(line);
                            }
                        }
                    }
                }

                return ExecutionResult.Success;
            } catch (RHostDisconnectedException rhdex) {
                WriteRHostDisconnectedError(rhdex);
                return ExecutionResult.Success;
            } catch (OperationCanceledException) {
                // Cancellation reason was already reported via RSession.Error and printed out;
                // Return success cause connection lost doesn't mean that RHost died
                return ExecutionResult.Success;
            } catch (Exception ex) {
                await _coreShell.ShowErrorMessageAsync(ex.ToString());
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

        public IInteractiveWindow CurrentWindow {
            get => _currentWindow;
            set {
                if (_currentWindow != null) {
                    _currentWindow.TextView.VisualElement.SizeChanged -= VisualElement_SizeChanged;
                }
                _currentWindow = value;
                if (_currentWindow != null) {
                    _currentWindow.TextView.VisualElement.SizeChanged += VisualElement_SizeChanged;
                    _windowWriter = new InteractiveWindowWriter(_coreShell.MainThread(), _currentWindow);
                }
            }
        }

        private void SessionOnOutput(object sender, ROutputEventArgs args) {
            if (args.OutputType == OutputType.Output) {
                Write(args.Message.ToUnicodeQuotes());
            } else {
                WriteError(args.Message.ToUnicodeQuotes());
            }
        }

        private void SessionOnConnected(object sender, EventArgs args) => _brokerChanging = false;

        private void SessionOnDisconnected(object sender, EventArgs args) {
            if (!_brokerChanging) {
                if (CurrentWindow == null || !CurrentWindow.IsResetting) {
                    WriteErrorLine(Environment.NewLine + Resources.MicrosoftRHostStopped);
                }
            } else {
                WriteErrorLine(Environment.NewLine + Resources.BrokerDisconnected);
                _brokerChanging = false;
            }
        }

        private void SessionOnAfterRequest(object sender, RAfterRequestEventArgs e) {
            if (CurrentWindow == null || CurrentWindow.IsResetting) {
                return;
            }

            if (_evaluatorRequest.Count == 0 && e.AddToHistory && e.IsVisible) {
                _coreShell.MainThread().Post(() => {
                    if (CurrentWindow == null || CurrentWindow.IsResetting) {
                        return;
                    }

                    ((IInteractiveWindow2)CurrentWindow).AddToHistory(e.Request.TrimEnd());
                    History.AddToHistory(e.Request);
                });
            }
        }

        private void SessionOnBeforeRequest(object sender, RBeforeRequestEventArgs e) {
            if (CurrentWindow == null || CurrentWindow.IsRunning) {
                return;
            }

            _coreShell.MainThread().Post(() => {
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
            if (CurrentWindow != null && !string.IsNullOrEmpty(message)) {
                _windowWriter.WriteMessage(message);
            }
        }

        private void WriteError(string message) {
            if (CurrentWindow != null && !string.IsNullOrEmpty(message)) {
                _windowWriter.WriteError(message);
            }
        }

        private void WriteErrorLine(string message) {
            message = TrimExcessiveLineBreaks(message);
            _console.WriteErrorLine(message);
        }

        private void WriteRHostDisconnectedError(RHostDisconnectedException exception) {
            WriteErrorLine(Environment.NewLine + exception.Message);
            WriteErrorLine(_sessionProvider.IsConnected ? Resources.RestartRHost : Resources.ReconnectToBroker);
        }

        private void VisualElement_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e) {
            var width = (int)(e.NewSize.Width / CurrentWindow.TextView.FormattedLineSource.ColumnWidth);
            // From R docs:  Valid values are 10...10000 with default normally 80.
            _terminalWidth = Math.Max(10, Math.Min(10000, width));

            Session.OptionsSetWidthAsync(_terminalWidth)
                .SilenceException<RException>()
                .DoNotWait();
        }

        /// <summary>
        /// Prevents multiple line breaks in REPL when various components prepend and append
        /// extra line breaks to the error message. Limits output to 2 line breaks per message.
        /// </summary>
        private string TrimExcessiveLineBreaks(string message) {
            if (CurrentWindow?.CurrentLanguageBuffer == null) {
                return message.Trim();
            }

            // Trim all line breaks at the end of the message
            message = message.TrimEnd(CharExtensions.LineBreakChars);

            // Trim and count leading new lines in the message
            var newLineLength = Environment.NewLine.Length;
            var nlInMessage = 0;
            while (message.StartsWithOrdinal(Environment.NewLine) && message.Length > newLineLength) {
                nlInMessage++;
                message = message.Substring(newLineLength, message.Length - newLineLength);
            }

            if (nlInMessage > 0) {
                // Count line breaks in the beginning of the message and at the end 
                // of the line text buffer and ensure no more than 2.
                var snapshot = CurrentWindow.CurrentLanguageBuffer.CurrentSnapshot;
                var nlInBuffer = 0;
                for (var i = snapshot.Length - newLineLength; i >= 0; i -= newLineLength) {
                    if (!snapshot.GetText(i, newLineLength).EqualsOrdinal(Environment.NewLine)) {
                        break;
                    }
                    nlInBuffer++;
                }

                // allow no more than 2 combined
                for (var i = 0; i < Math.Min(2, nlInBuffer + nlInMessage); i++) {
                    message = Environment.NewLine + message;
                }
            }

            return message;
        }
    }
}