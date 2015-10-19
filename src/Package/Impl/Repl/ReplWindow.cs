using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Document;
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
        private static readonly Lazy<ReplWindow> _instance = new Lazy<ReplWindow>(() => new ReplWindow());

        public ReplWindow() {
            IVsUIShell7 shell = AppShell.Current.GetGlobalService<IVsUIShell7>(typeof(SVsUIShell));
            _windowFrameEventsCookie = shell.AdviseWindowFrameEvents(this);
        }


        public static ReplWindow Current => _instance.Value;

        /// <summary>
        /// Inserts the provided code into the current input buffer at the current caret location.
        /// 
        /// If the current input becomes complete after inserting the code then the input is executed.  
        /// 
        /// If the code is not complete and addNewLine is true then a new line character is appended 
        /// to the end of the input.
        /// </summary>
        /// <param name="code">The code to be inserted</param>
        /// <param name="addNewLine">True to add a new line on non-complete inputs.</param>
        public void InsertCodeMaybeExecute(string code, bool addNewLine)
        {
            IVsInteractiveWindow current = _instance.Value.GetInteractiveWindow();
            if (current != null)
            {
                current.InteractiveWindow.InsertCode(code);
                var fullCode = current.InteractiveWindow.CurrentLanguageBuffer.CurrentSnapshot.GetText();
                
                if (current.InteractiveWindow.Evaluator.CanExecuteCode(fullCode))
                {
                    // the code is complete, go ahead and execute it...
                    current.InteractiveWindow.Operations.ExecuteInput();
                }
                else if (addNewLine)
                {
                    // We want a new line after non-complete inputs, e.g. the user ctrl-entered on
                    // function() {
                    current.InteractiveWindow.InsertCode(current.InteractiveWindow.TextView.Options.GetNewLineCharacter());
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
                if (!documentPoint.HasValue) {
                    current.InteractiveWindow.Operations.Return();
                } else {
                    if (documentPoint.Value == documentPoint.Value.Snapshot.Length || documentPoint.Value.Snapshot.Length == 0) {
                        current.InteractiveWindow.Operations.Return();
                    } else {
                        current.InteractiveWindow.Operations.ExecuteInput();
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
                    _lastUsedReplWindow = docView as IVsInteractiveWindow;
                    return _lastUsedReplWindow != null;
                }
            }

            return false;
        }
    }
}
