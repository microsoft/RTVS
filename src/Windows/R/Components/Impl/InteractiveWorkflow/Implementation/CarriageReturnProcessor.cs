// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    /// <summary>
    /// Works around the fact that interactive window does not support
    /// carriage returns as mean to type over strings such as when
    /// displaying ASCII progress bar.
    /// </summary>
    internal sealed class CarriageReturnProcessor : IDisposable {
        private readonly Queue<Message> _messages = new Queue<Message>();
        private readonly ManualResetEventSlim _messagesAvailable = new ManualResetEventSlim(false);
        public readonly IServiceContainer _services;
        public readonly IMainThread _mainThread;
        private readonly IInteractiveWindow _interactiveWindow;
        private readonly object _lock = new object();
        private int _currentLineStart = -1;
        private volatile bool _disposed;

        private class Message {
            public string Text;
            public bool IsError;

            public bool MovesCaretBack() => Text.Length > 1 && Text[0] == '\r' && Text[1] != '\n';
            public bool ContainsLineFeed() => Text.Any(ch => ch == '\n');
        }

        public CarriageReturnProcessor(IServiceContainer services, IInteractiveWindow interactiveWindow) {
            _services = services;
            _mainThread = _services.MainThread();
            _interactiveWindow = interactiveWindow;
            Task.Run(async () => await OutputProcessingTask()).DoNotWait();
        }

        public void Dispose() {
            _disposed = true;
        }

        public void ProcessMessage(string message) {
            lock (_lock) {
                _messages.Enqueue(new Message { Text = message, IsError = false });
                _messagesAvailable.Set();
            }
        }

        public void ProcessError(string message) {
            lock (_lock) {
                _messages.Enqueue(new Message { Text = message, IsError = true });
                _messagesAvailable.Set();
            }
        }

        private async Task OutputProcessingTask() {
            TaskUtilities.AssertIsOnBackgroundThread();

            while (!_disposed) {
                _messagesAvailable.Wait();

                await _mainThread.SwitchToAsync();
                lock (_lock) {
                    foreach (var message in GetMessages(20)) {

                        if (message.ContainsLineFeed()) {
                            _currentLineStart = -1;
                        }

                        if (message.IsError) {
                            _interactiveWindow.WriteError(message.Text);
                            _interactiveWindow.FlushOutput();
                        } else if (message.MovesCaretBack()) {
                            ProcessCarriageReturn(message.Text);
                        } else {
                            _interactiveWindow.Write(message.Text);
                            _interactiveWindow.FlushOutput();
                        }
                    }

                    _interactiveWindow.FlushOutput();
                    _messagesAvailable.Reset();
                }

                await TaskUtilities.SwitchToBackgroundThread();
            }
        }

        private IEnumerable<Message> GetMessages(int maxCount) {
            var list = new List<Message>();
            var lastCrIndex = -1;

            while (_messages.Count > 0) {
                var m = _messages.Dequeue();
                list.Add(m);

                if (m.ContainsLineFeed()) {
                    break;
                }

                if (m.MovesCaretBack()) {
                    if (list.Count > maxCount) {
                        break;
                    }

                    // Trim previous CR since new one overlaps it
                    if (lastCrIndex >= 0) {
                        list.RemoveRange(lastCrIndex, list.Count - lastCrIndex - 1);
                    }
                    lastCrIndex = list.Count - 1;
                }
            }

            return list;
        }

        private void ProcessCarriageReturn(string message) {
            // Make sure output buffer is up to date
            _interactiveWindow.FlushOutput();

            // If message starts with CR we remember current output buffer
            // length so we can continue writing lines into the same spot.
            // See txtProgressBar in R.
            // Store the message and the initial position. All subsequent 
            // messages that start with CR will be written into the same place.
            if (_currentLineStart >= 0) {
                // Delete text on the line written so far. Issue is that the buffer is readonly. 
                // So we write space and handle actual delete in the event handler where buffer 
                // is still writeable.
                _interactiveWindow.OutputBuffer.Changed += OnBufferChanged;
                _interactiveWindow.Write(" ");
                // Must flush so we do get 'buffer changed' immediately.
                _interactiveWindow.FlushOutput();
            }

            Debug.Assert(_currentLineStart == -1 || _interactiveWindow.OutputBuffer.CurrentSnapshot.Length == _currentLineStart);

            // Locate last end of line
            var snapshot = _interactiveWindow.OutputBuffer.CurrentSnapshot;
            var line = snapshot.GetLineFromPosition(snapshot.Length);
            _currentLineStart = line.Start;

            // It is important that replacement matches original text length
            // since interactive window creates fixed colorized spans for errors
            // and replacement of text by a text with a different length
            // causes odd changes in color - word may appear partially in
            // black and partially in red.

            // Replacement placeholder so we can receive 'buffer changed' event
            // Placeholder is whitespace that is as long as original message plus
            // few more space to account for example, for 0% - 100% when CR is used
            // to display ASCII progress.
            _interactiveWindow.Write(message.Substring(1));
        }

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e) {
            _interactiveWindow.OutputBuffer.Changed -= OnBufferChanged;

            Debug.Assert(_currentLineStart >= 0);
            if (_currentLineStart >= 0) {
                var end = _interactiveWindow.OutputBuffer.CurrentSnapshot.Length;
                _interactiveWindow.OutputBuffer.Delete(Span.FromBounds(_currentLineStart, end));
            }
        }
    }
}
