// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class InteractiveWindowMock : IInteractiveWindow {

        private ITextBuffer _textBuffer;

        public InteractiveWindowMock(IWpfTextView textView, IInteractiveEvaluator evaluator = null) {
            TextView = textView;
            _textBuffer = textView.TextBuffer;
            Evaluator = evaluator ?? new InteractiveEvaluatorMock(this);
        }

        public ITextBuffer CurrentLanguageBuffer {
            get {
                return _textBuffer;
            }
        }

        public TextWriter ErrorOutputWriter {
            get {
                throw new NotImplementedException();
            }
        }

        public IInteractiveEvaluator Evaluator { get; }

        public bool IsInitializing => false;

        public bool IsResetting => false;

        public bool IsRunning => false;

        public IInteractiveWindowOperations Operations => new InteractiveWindowOperationsMock(_textBuffer);

        public ITextBuffer OutputBuffer => _textBuffer;

        public TextWriter OutputWriter {
            get {
                throw new NotImplementedException();
            }
        }

        public PropertyCollection Properties { get; } = new PropertyCollection();

        public IWpfTextView TextView { get; private set; }

#pragma warning disable 67
        public event Action ReadyForInput;
        public event EventHandler<SubmissionBufferAddedEventArgs> SubmissionBufferAdded;

        public void AddInput(string input) {
            throw new NotImplementedException();
        }

        public void Close() {
        }

        public void Dispose() {
        }

        public void FlushOutput() {
        }

        public System.Threading.Tasks.Task<ExecutionResult> InitializeAsync() {
            return Evaluator.InitializeAsync();
        }

        public void InsertCode(string text) {
            _textBuffer.Insert(_textBuffer.CurrentSnapshot.Length, text);
        }

        public TextReader ReadStandardInput() {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task SubmitAsync(IEnumerable<string> inputs) {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public void Write(System.Windows.UIElement element) {
            throw new NotImplementedException();
        }

        public Span Write(string text) {
            InsertCode(text);
            return new Span(0, _textBuffer.CurrentSnapshot.Length);
        }

        public Span WriteError(string text) {
            return Write(text);
        }

        public Span WriteErrorLine(string text) {
            return Write(text);
        }

        public Span WriteLine(string text) {
            return Write(text);
        }
    }
}
