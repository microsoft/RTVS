using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class InteractiveWindowMock : IInteractiveWindow {

        private ITextBuffer _textBuffer;

        public InteractiveWindowMock(IWpfTextView textView) {
            TextView = textView;
            _textBuffer = textView.TextBuffer;
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

        public IInteractiveEvaluator Evaluator {
            get {
                throw new NotImplementedException();
            }
        }

        public bool IsInitializing {
            get {
                throw new NotImplementedException();
            }
        }

        public bool IsResetting {
            get {
                throw new NotImplementedException();
            }
        }

        public bool IsRunning {
            get {
                throw new NotImplementedException();
            }
        }

        public IInteractiveWindowOperations Operations {
            get {
                throw new NotImplementedException();
            }
        }

        public ITextBuffer OutputBuffer {
            get {
                throw new NotImplementedException();
            }
        }

        public TextWriter OutputWriter {
            get {
                throw new NotImplementedException();
            }
        }

        public PropertyCollection Properties {
            get {
                throw new NotImplementedException();
            }
        }

        public IWpfTextView TextView { get; private set; }

#pragma warning disable 67
        public event Action ReadyForInput;
        public event EventHandler<SubmissionBufferAddedEventArgs> SubmissionBufferAdded;

        public void AddInput(string input) {
            throw new NotImplementedException();
        }

        public void Close() {
            throw new NotImplementedException();
        }

        public void Dispose() {
            throw new NotImplementedException();
        }

        public void FlushOutput() {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<ExecutionResult> InitializeAsync() {
            return System.Threading.Tasks.Task.FromResult(ExecutionResult.Success);
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
            throw new NotImplementedException();
        }

        public Span WriteError(string text) {
            throw new NotImplementedException();
        }

        public Span WriteErrorLine(string text) {
            throw new NotImplementedException();
        }

        public Span WriteLine(string text) {
            throw new NotImplementedException();
        }
    }
}
