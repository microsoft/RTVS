using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks.Dataflow;
using Microsoft.Languages.Editor.Shell;
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
using Microsoft.VisualStudio.Text.Operations;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Repl {
    /// <summary>
    /// Tracks most recently active REPL window
    /// </summary>
    internal sealed class ReplWindow : IVsWindowFrameEvents, IDisposable {
        private uint _windowFrameEventsCookie;
        private IVsInteractiveWindow _lastUsedReplWindow;
        private static readonly Lazy<ReplWindow> _instance = new Lazy<ReplWindow>(() => new ReplWindow());
        LinkedList<PendingSubmission> _pendingInputs = new LinkedList<PendingSubmission>();

        class PendingSubmission {
            public string Code;
            public bool AddNewLine;
        }

        public ReplWindow() {
            IVsUIShell7 shell = AppShell.Current.GetGlobalService<IVsUIShell7>(typeof(SVsUIShell));
            _windowFrameEventsCookie = shell.AdviseWindowFrameEvents(this);
        }

        public static ReplWindow Current => _instance.Value;

        private void ProcessQueuedInput() {
            IVsInteractiveWindow interactive = _instance.Value.GetInteractiveWindow();
            if (interactive != null) {
                var window = interactive.InteractiveWindow;

                // Process all of our pending inputs until we get a complete statement
                while (_pendingInputs.Count != 0) {
                    var cur = _pendingInputs.First.Value;
                    _pendingInputs.RemoveFirst();

                    window.InsertCode(cur.Code);
                    string fullCode = window.CurrentLanguageBuffer.CurrentSnapshot.GetText();

                    if (window.Evaluator.CanExecuteCode(fullCode)) {
                        // the code is complete, execute it now
                        window.Operations.ExecuteInput();
                        break;
                    } else if (cur.AddNewLine) {
                        // We want a new line after non-complete inputs, e.g. the user ctrl-entered on
                        // function() {
                        window.InsertCode(window.TextView.Options.GetNewLineCharacter());
                    }
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
            IVsInteractiveWindow current = _instance.Value.GetInteractiveWindow();
            if (current != null) {
                if (current.InteractiveWindow.IsResetting) {
                    return;
                }

                // add the input to our queue...
                _pendingInputs.AddLast(new PendingSubmission() { Code = code, AddNewLine = addNewLine });

                if (!current.InteractiveWindow.IsRunning) {
                    // and process the queue if we weren't currently running
                    ProcessQueuedInput();
                }
            }
        }

        public void ExecuteCode(string code) {
            IVsInteractiveWindow current = _instance.Value.GetInteractiveWindow();
            if (current != null && !string.IsNullOrWhiteSpace(code)) {
                current.InteractiveWindow.AddInput(code);
                current.InteractiveWindow.Operations.ExecuteInput();
            }
        }

        public void ExecuteCurrentExpression(ITextView textView) {
            ICompletionBroker broker = EditorShell.Current.ExportProvider.GetExport<ICompletionBroker>().Value;
            broker.DismissAllSessions(textView);

            IVsInteractiveWindow current = _instance.Value.GetInteractiveWindow();
            if (current != null) {
                SnapshotPoint? documentPoint = REditorDocument.MapCaretPositionFromView(textView);
                if (!documentPoint.HasValue ||
                    documentPoint.Value == documentPoint.Value.Snapshot.Length ||
                    documentPoint.Value.Snapshot.Length == 0) {
                    // Let the repl try and execute the code if the user presses enter at the
                    // end of the buffer.
                    current.InteractiveWindow.Operations.Return();
                } else {
                    // Otherwise insert a line break in the middle of an input
                    current.InteractiveWindow.Operations.BreakLine();
                    var document = REditorDocument.TryFromTextBuffer(current.InteractiveWindow.CurrentLanguageBuffer);
                    if (document != null) {
                        var tree = document.EditorTree;
                        tree.EnsureTreeReady();

                        AutoFormat.HandleAutoFormat(
                            current.InteractiveWindow.TextView,
                            current.InteractiveWindow.CurrentLanguageBuffer,
                            tree.AstRoot,
                            '\n'
                        );
                    }
                }
            }
        }

        public IVsInteractiveWindow GetInteractiveWindow() {
            if (_lastUsedReplWindow == null) {
                IVsWindowFrame frame;
                IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));

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
            if (frame != null) {
                frame.Show();
            }
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
            IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));

            Guid persistenceSlot = RGuidList.ReplInteractiveWindowProviderGuid;

            // First just find. If it exists, use it. 
            shell.FindToolWindow((uint)flags, ref persistenceSlot, out frame);
            return frame;
        }

        #region IVsWindowFrameEvents
        public void OnFrameCreated(IVsWindowFrame frame) {
        }

        public void OnFrameDestroyed(IVsWindowFrame frame) {
            if (_lastUsedReplWindow == frame) {
                _lastUsedReplWindow = null;
            }
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
                IVsUIShell7 shell = AppShell.Current.GetGlobalService<IVsUIShell7>(typeof(SVsUIShell));
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
                        _lastUsedReplWindow.InteractiveWindow.ReadyForInput += ProcessQueuedInput;
                    }
                    return _lastUsedReplWindow != null;
                }
            }

            return false;
        }
    }
}
