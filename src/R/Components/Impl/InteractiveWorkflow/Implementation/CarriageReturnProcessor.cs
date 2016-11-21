// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    /// <summary>
    /// Works around the fact that interactive window does not support
    /// carriage returns as mean to type over strings such as when
    /// displaying ASCII progress bar.
    /// </summary>
    internal sealed class CarriageReturnProcessor : IDisposable {
        public readonly ICoreShell _coreShell;
        private readonly IInteractiveWindow _interactiveWindow;
        private MessagePos _messagePos;

        /// <summary>
        /// Workaround for interactive window that does not currently support
        /// 'carriage return' i.e. writing into the same line
        /// </summary>
        class MessagePos {
            public string Text;
            public int Position;
            public int PlaceholderLength;
        }

        public CarriageReturnProcessor(ICoreShell coreShell, IInteractiveWindow interactiveWindow) {
            _coreShell = coreShell;
            _interactiveWindow = interactiveWindow;
            interactiveWindow.OutputBuffer.Changed += OnBufferChanged;
        }

        public void Dispose() {
            _interactiveWindow.OutputBuffer.Changed -= OnBufferChanged;
        }

        public bool ProcessMessage(string message) {
            // Note: DispatchOnUIThread is expensive, and can saturate the message pump when there's a lot of output,
            // making UI non-responsive. So avoid using it unless we need it - and we only need it for FlushOutput,
            // and we only need it to handle CR.
            if (message.Length > 1 && message[0] == '\r' && message[1] != '\n') {
                _coreShell.DispatchOnUIThread(() => {
                    // Make sure output buffer is up to date
                    _interactiveWindow.FlushOutput();

                    // If message starts with CR we remember current output buffer
                    // length so we can continue writing lines into the same spot.
                    // See txtProgressBar in R.
                    // Store the message and the initial position. All subsequent 
                    // messages that start with CR will be written into the same place.
                    if (_messagePos != null) {
                        ProcessReplacement();
                    }

                    // Locate last end of line
                    var snapshot = _interactiveWindow.OutputBuffer.CurrentSnapshot;
                    var line = snapshot.GetLineFromPosition(snapshot.Length);
                    var text = message.Substring(1);

                    _messagePos = new MessagePos() {
                        Text = text,
                        Position = line.Start,
                        PlaceholderLength = text.Length + 8 // buffer for, say, '| 100%'
                    };

                    // It is important that replacement matches original text length
                    // since interactive window creates fixed colorized spans for errors
                    // and replacement of text by a text with a different length
                    // causes odd changes in color - word may appear partially in
                    // black and partially in red.
                     
                    // Replacement placeholder so we can receive 'buffer changed' event
                    // Placeholder is whitespace that is as long as original message plus
                    // few more space to account for example, for 0% - 100% when CR is used
                    // to display ASCII progress.
                    var placeholder = new string(' ', _messagePos.PlaceholderLength);

                    _interactiveWindow.Write(placeholder);
                    _interactiveWindow.FlushOutput(); // Must flush so we do get 'buffer changed' immediately.
                });
                return true;
            }
            return false;
        }

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e) {
            if (_messagePos != null) {
                ProcessReplacement();
            }
        }

        private void ProcessReplacement() {
            _coreShell.AssertIsOnMainThread();
            // Writing messages in the same line (via simulated CR)

            var m = _messagePos;
            _messagePos = null;

            // Replace last written placeholder with the actual message.
            // Pad text as necessary by spaces to match original length.
            var extra = m.PlaceholderLength - m.Text.Length;
            var replacement = m.Text + (extra > 0 ? new string(' ', extra) : string.Empty);
            _interactiveWindow.OutputBuffer.Replace(new Span(m.Position, m.PlaceholderLength), replacement);
        }
    }
}
