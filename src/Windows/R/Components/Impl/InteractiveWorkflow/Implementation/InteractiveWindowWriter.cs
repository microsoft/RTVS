// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Threading;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    /// <summary>
    /// Works around the fact that interactive window does not support
    /// carriage returns as mean to type over strings such as when
    /// displaying ASCII progress bar.
    /// </summary>
    internal sealed class InteractiveWindowWriter : IDisposable {
        private readonly MessageQueue _messageQueue = new MessageQueue();
        private readonly IMainThread _mainThread;
        private readonly IInteractiveWindow _interactiveWindow;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private volatile bool _disposed;

        private int _currentLineStart = -1;
        private string _newText;

        public InteractiveWindowWriter(IMainThread mainThread, IInteractiveWindow interactiveWindow) {
            _mainThread = mainThread;
            _interactiveWindow = interactiveWindow;
            OutputProcessingTask().DoNotWait();
        }

        public void Dispose() {
            _cts.Cancel();
            _messageQueue.Dispose();
            _disposed = true;
        }

        public void WriteMessage(string message) => _messageQueue.Enqueue(message, false);
        public void WriteError(string message) => _messageQueue.Enqueue(message, true);

        private async Task OutputProcessingTask() {
            while (!_disposed) {
                var messages = await _messageQueue.WaitForMessagesAsync(_cts.Token);
                await _mainThread.SwitchToAsync(_cts.Token);

                foreach (var m in messages) {
                    if (m.IsError) {
                        _interactiveWindow.WriteError(m.Text);
                    } else if (m.MovesCaretBack()) {
                        ProcessCarriageReturn(m.Text);
                    } else {
                        _interactiveWindow.Write(m.Text);
                    }
                }
            }
        }


        private void ProcessCarriageReturn(string message) {
            ITextSnapshot snapshot;
            var text = message.Substring(1);

            // Make sure output buffer is up to date
            _interactiveWindow.FlushOutput();

            // If message starts with CR we remember current output buffer
            // length so we can continue writing lines into the same spot.
            // See txtProgressBar in R.
            // Store the message and the initial position. All subsequent 
            // messages that start with CR will be written into the same place.
            if (_currentLineStart >= 0) {
                snapshot = _interactiveWindow.OutputBuffer.CurrentSnapshot;
                var span = Span.FromBounds(_currentLineStart, snapshot.Length);
                if (span.Length == text.Length && snapshot.GetText(span).EqualsOrdinal(text)) {
                    return; // Same text, ignore
                }

                _newText = text;
                // Delete text on the line written so far. Issue is that the buffer is readonly. 
                // So we write space and handle actual delete in the event handler where buffer 
                // is still writeable.
                _interactiveWindow.OutputBuffer.Changed += OnBufferChanged;
                _interactiveWindow.Write(" ");
                // Must flush so we do get 'buffer changed' immediately.
                _interactiveWindow.FlushOutput();
            } else {
                // Locate last end of line
                snapshot = _interactiveWindow.OutputBuffer.CurrentSnapshot;
                var line = snapshot.GetLineFromPosition(snapshot.Length);
                _currentLineStart = line.Start;

            }
        }

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e) {
            _interactiveWindow.OutputBuffer.Changed -= OnBufferChanged;

            Debug.Assert(_currentLineStart >= 0);
            if (_currentLineStart >= 0) {
                var snapshot = _interactiveWindow.OutputBuffer.CurrentSnapshot;

                var span = Span.FromBounds(_currentLineStart, snapshot.Length);
                var currentText = snapshot.GetText(span);

                var index = GetDifference(currentText, _newText, out string difference);
                if (index >= 0) {
                    span = Span.FromBounds(_currentLineStart + index, snapshot.Length);
                    _interactiveWindow.OutputBuffer.Replace(span, difference);
                } else {
                    _interactiveWindow.OutputBuffer.Replace(span, _newText);
                }
            }
        }

        private static int GetDifference(string oldText, string newText, out string difference) {
            difference = string.Empty;
            if (newText.Length < oldText.Length) {
                return -1;
            }

            var index = -1;

            for (var i = 0; i < oldText.Length; i++) {
                if (oldText[i] != newText[i]) {
                    index = i;
                    break;
                }
            }

            difference = newText.Substring(index);
            return index;
        }

        internal class Message {
            public string Text;
            public bool IsError;

            public bool MovesCaretBack() => Text.Length > 1 && Text[0] == '\r' && Text[1] != '\n';
            public bool ContainsLineFeed() => Text.Any(ch => ch == '\n');
            public bool IsPlainText() => !MovesCaretBack() && !ContainsLineFeed();
        }

        internal class MessageQueue : IDisposable {
            private readonly List<Message> _messages = new List<Message>();
            private readonly AsyncManualResetEvent _messagesAvailable = new AsyncManualResetEvent();
            private readonly object _lock = new object();
            private int _lastCRMessageIndex = -1;

            public void Enqueue(string text, bool isError) {
                lock (_lock) {
                    var m = new Message { Text = text, IsError = isError };
                    // Try merging text with the last CR message, if any
                    if (_messages.Count > 0) {
                        var lastMessage = _messages[_messages.Count - 1];
                        if (lastMessage.MovesCaretBack() && m.IsPlainText() && lastMessage.IsError == m.IsError) {
                            lastMessage.Text += text;
                            return;
                        }
                    }

                    if (m.MovesCaretBack()) {
                        // CR moves caret back effectively erasing previous message at the line
                        if (_lastCRMessageIndex >= 0) {
                            _messages.RemoveRange(_lastCRMessageIndex, _messages.Count - _lastCRMessageIndex);
                        }
                        _lastCRMessageIndex = _messages.Count;
                    } else if (m.ContainsLineFeed()) {
                        _lastCRMessageIndex = -1;
                    }

                    _messages.Add(m);
                    _messagesAvailable.Set();
                }
            }

            public async Task<IEnumerable<Message>> WaitForMessagesAsync(CancellationToken ct) {
                await TaskUtilities.SwitchToBackgroundThread();

                await _messagesAvailable.WaitAsync(ct);
                if(ct.IsCancellationRequested) {
                    return Enumerable.Empty<Message>();
                }

                await Task.Delay(200, ct); // Throttle output a bit to reduce flicker
                lock (_lock) {
                    var array = _messages.ToArray();
                    _messages.Clear();
                    _lastCRMessageIndex = -1;
                    _messagesAvailable.Reset();
                    return array;
                }
            }

            public void Dispose() {
                _messagesAvailable.Set();
            }
        }
    }
}
