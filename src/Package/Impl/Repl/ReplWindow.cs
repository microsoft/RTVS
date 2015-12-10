using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Formatting;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Repl {
    /// <summary>
    /// Tracks most recently active REPL window
    /// </summary>
    internal sealed class ReplWindow : IVsWindowFrameEvents, IDisposable {
        private uint _windowFrameEventsCookie;
        private IVsInteractiveWindow _lastUsedReplWindow;
        private ConcurrentQueue<PendingSubmission> _pendingInputs = new ConcurrentQueue<PendingSubmission>();
        private static readonly Lazy<ReplWindow> Instance = new Lazy<ReplWindow>(() => new ReplWindow());

        class PendingSubmission {
            public string Code;
            public bool AddNewLine;
        }

        public ReplWindow() {
            IVsUIShell7 shell = VsAppShell.Current.GetGlobalService<IVsUIShell7>(typeof(SVsUIShell));
            _windowFrameEventsCookie = shell.AdviseWindowFrameEvents(this);
        }

        public static ReplWindow Current => Instance.Value;

        public bool IsActive {
            get {
                IVsWindowFrame frame = Current.GetToolWindow();
                if (frame != null) {
                    int onScreen;
                    frame.IsOnScreen(out onScreen);
                    return onScreen != 0;
                }
                return false;
            }
        }

        public IVsWindowFrame GetToolWindow() {
            IVsWindowFrame frame;
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            Guid persistenceSlot = RGuidList.ReplInteractiveWindowProviderGuid;

            // First just find. If it exists, use it. 
            shell.FindToolWindow((int)__VSFINDTOOLWIN.FTW_fFindFirst, ref persistenceSlot, out frame);
            return frame;
        }

        private void ProcessQueuedInput() {
            IVsInteractiveWindow interactive = Instance.Value.GetInteractiveWindow();
            if (interactive == null) {
                return;
            }

            var window = interactive.InteractiveWindow;
            var view = interactive.InteractiveWindow.TextView;

            // Process all of our pending inputs until we get a complete statement
            PendingSubmission current;
            while (_pendingInputs.TryDequeue(out current)) {
                var curLangBuffer = interactive.InteractiveWindow.CurrentLanguageBuffer;

                var curLangPoint = view.MapDownToBuffer(
                    interactive.InteractiveWindow.CurrentLanguageBuffer.CurrentSnapshot.Length,
                    curLangBuffer
                );
                if (curLangPoint == null) {
                    // ensure the caret is in the input buffer, otherwise inserting code does nothing
                    view.Caret.MoveTo(
                        view.BufferGraph.MapUpToBuffer(
                            new SnapshotPoint(
                                curLangBuffer.CurrentSnapshot, curLangBuffer.CurrentSnapshot.Length
                            ),
                            PointTrackingMode.Positive,
                            PositionAffinity.Successor,
                            view.TextBuffer
                        ).Value
                    );
                }

                window.InsertCode(current.Code);
                string fullCode = curLangBuffer.CurrentSnapshot.GetText();

                if (window.Evaluator.CanExecuteCode(fullCode)) {
                    // the code is complete, execute it now
                    window.Operations.ExecuteInput();
                    return;
                }

                if (current.AddNewLine) {
                    // We want a new line after non-complete inputs, e.g. the user ctrl-entered on
                    // function() {
                    window.InsertCode(view.Options.GetNewLineCharacter());
                }
            }
        }

        /// <summary>
        /// Enqueues the provided code for execution.  If there's no current execution the code is
        /// inserted at the caret position.  Otherwise the code is stored for when the current
        /// execution is completed.
        /// 
        /// If the current input becomes complete after inserting the code then the input is executed.  
        /// 
        /// If the code is not complete and addNewLine is true then a new line character is appended 
        /// to the end of the input.
        /// </summary>
        /// <param name="code">The code to be inserted</param>
        /// <param name="addNewLine">True to add a new line on non-complete inputs.</param>
        public void EnqueueCode(string code, bool addNewLine) {
            IVsInteractiveWindow current = Instance.Value.GetInteractiveWindow();
            if (current != null) {
                if (current.InteractiveWindow.IsResetting) {
                    return;
                }

                // add the input to our queue...
                _pendingInputs.Enqueue(new PendingSubmission { Code = code, AddNewLine = addNewLine });

                if (!current.InteractiveWindow.IsRunning) {
                    // and process the queue if we weren't currently running
                    ProcessQueuedInput();
                }
            }
        }

        public void ClearPendingInputs() {
            Interlocked.Exchange(ref _pendingInputs, new ConcurrentQueue<PendingSubmission>());
        }

        public void ExecuteCode(string code) {
            IVsInteractiveWindow current = Instance.Value.GetInteractiveWindow();
            if (current != null && !string.IsNullOrWhiteSpace(code)) {
                current.InteractiveWindow.AddInput(code);
                current.InteractiveWindow.Operations.ExecuteInput();
            }
        }

        public void ReplaceCurrentExpression(string replaceWith) {
            IVsInteractiveWindow current = Instance.Value.GetInteractiveWindow();
            if (current != null) {
                var textBuffer = current.InteractiveWindow.CurrentLanguageBuffer;
                var span = new Span(0, textBuffer.CurrentSnapshot.Length);
                if (!textBuffer.IsReadOnly(span)) {
                    textBuffer.Replace(span, replaceWith);
                }
            }
        }

        public void ExecuteCurrentExpression(ITextView textView) {
            ICompletionBroker broker = VsAppShell.Current.ExportProvider.GetExportedValue<ICompletionBroker>();
            broker.DismissAllSessions(textView);

            IVsInteractiveWindow current = Instance.Value.GetInteractiveWindow();
            if (current != null && !current.InteractiveWindow.IsRunning) {
                var curBuffer = current.InteractiveWindow.CurrentLanguageBuffer;
                SnapshotPoint? documentPoint = textView.MapDownToBuffer(textView.Caret.Position.BufferPosition, curBuffer);
                var text = curBuffer.CurrentSnapshot.GetText();
                if (!documentPoint.HasValue ||
                    documentPoint.Value == documentPoint.Value.Snapshot.Length ||
                    documentPoint.Value.Snapshot.Length == 0 ||
                    !IsMultiLineCandidate(text)) {
                    // Let the repl try and execute the code if the user presses enter at the
                    // end of the buffer.
                    if (current.InteractiveWindow.Evaluator.CanExecuteCode(text)) {
                        // If we know we can execute the code move the caret to the end of the
                        // current input, otherwise the interactive window won't execute it.  We
                        // have slightly more permissive handling here.
                        var point = textView.BufferGraph.MapUpToBuffer(
                            new SnapshotPoint(
                                curBuffer.CurrentSnapshot,
                                curBuffer.CurrentSnapshot.Length
                            ),
                            PointTrackingMode.Positive,
                            PositionAffinity.Successor,
                            textView.TextBuffer
                        );
                        textView.Caret.MoveTo(point.Value);
                    }

                    current.InteractiveWindow.Operations.Return();
                } else {
                    // Otherwise insert a line break in the middle of an input
                    current.InteractiveWindow.Operations.BreakLine();
                    var document = REditorDocument.TryFromTextBuffer(curBuffer);
                    if (document != null) {
                        var tree = document.EditorTree;
                        tree.EnsureTreeReady();

                        FormatOperations.FormatNode<IStatement>(
                            current.InteractiveWindow.TextView,
                            curBuffer,
                            Math.Max(documentPoint.Value - 1, 0)
                        );
                    }
                }
            }
        }

        private static bool IsMultiLineCandidate(string text) {
            if (text.IndexOfAny(new[] { '\n', '\r' }) != -1) {
                // if we already have newlines then we're multiline
                return true;
            }

            var tokenizer = new RTokenizer();
            IReadOnlyTextRangeCollection<RToken> tokens = tokenizer.Tokenize(
                new TextStream(text), 0, text.Length);
            return tokens.Any(t => t.TokenType == RTokenType.OpenCurlyBrace);
        }

        public IVsInteractiveWindow GetInteractiveWindow() {
            if (_lastUsedReplWindow == null) {
                IVsWindowFrame frame;
                IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));

                Guid persistenceSlot = RGuidList.ReplInteractiveWindowProviderGuid;

                // First just find. If it exists, use it. 
                shell.FindToolWindow((int)__VSFINDTOOLWIN.FTW_fFindFirst, ref persistenceSlot, out frame);
                if (frame == null) {
                    shell.FindToolWindow((int)__VSFINDTOOLWIN.FTW_fForceCreate, ref persistenceSlot, out frame);
                }

                if (frame != null) {
                    frame.Show();
                    CheckReplFrame(frame);
                }
            }

            return _lastUsedReplWindow;
        }

        public static bool ReplWindowExists() {
            IVsWindowFrame frame = FindReplWindowFrame(__VSFINDTOOLWIN.FTW_fFindFirst);
            return frame != null;
        }

        public static void Show() {
            IVsWindowFrame frame = FindReplWindowFrame(__VSFINDTOOLWIN.FTW_fFindFirst);
            frame?.Show();
        }

        public static async Task EnsureReplWindow() {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!ReplWindowExists()) {
                IVsWindowFrame frame = FindReplWindowFrame(__VSFINDTOOLWIN.FTW_fForceCreate);
                if (frame != null) {
                    //IntPtr bitmap = Resources.ReplWindowIcon.GetHbitmap();
                    frame.SetProperty((int)__VSFPROPID4.VSFPROPID_TabImage, Resources.ReplWindowIcon);
                    frame.Show();
                }
            }
        }

        public static IVsWindowFrame FindReplWindowFrame(__VSFINDTOOLWIN flags) {
            IVsWindowFrame frame;
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));

            Guid persistenceSlot = RGuidList.ReplInteractiveWindowProviderGuid;

            // First just find. If it exists, use it. 
            shell.FindToolWindow((uint)flags, ref persistenceSlot, out frame);
            return frame;
        }

        #region IVsWindowFrameEvents
        public void OnFrameCreated(IVsWindowFrame frame) {

        }

        public void OnFrameDestroyed(IVsWindowFrame frame) {
            
        }

        public void OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible) {
        }

        public void OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen) {
        }

        public void OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame) {
            // Track last recently used REPL window
            if (!CheckReplFrame(oldFrame)) {
                CheckReplFrame(newFrame);
            }
        }
        #endregion

        #region IDisposable
        public void Dispose() {
            if (_windowFrameEventsCookie != 0) {
                IVsUIShell7 shell = VsAppShell.Current.GetGlobalService<IVsUIShell7>(typeof(SVsUIShell));
                shell.UnadviseWindowFrameEvents(_windowFrameEventsCookie);
                _windowFrameEventsCookie = 0;
            }

            _lastUsedReplWindow = null;
        }
        #endregion

        private bool CheckReplFrame(IVsWindowFrame frame) {
            if (frame != null) {
                Guid property;
                frame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_GuidPersistenceSlot, out property);
                if (property == RGuidList.ReplInteractiveWindowProviderGuid) {
                    object docView;
                    frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView);
                    if (_lastUsedReplWindow != null) {
                        _lastUsedReplWindow.InteractiveWindow.ReadyForInput -= ProcessQueuedInput;
                    }
                    _lastUsedReplWindow = docView as IVsInteractiveWindow;
                    if (_lastUsedReplWindow != null) {
                        IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                        shell.UpdateCommandUI(1);
                        _lastUsedReplWindow.InteractiveWindow.ReadyForInput += ProcessQueuedInput;
                    }
                    return _lastUsedReplWindow != null;
                }
            }

            return false;
        }
    }
}
