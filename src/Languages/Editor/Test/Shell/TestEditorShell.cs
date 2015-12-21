using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Test.Shell {
    [ExcludeFromCodeCoverage]
    sealed class TestEditorShell : IEditorShell {
        private static TestEditorShell _instance;
        private static readonly object _lock = new object();

        public TestEditorShell(ICompositionCatalog catalog) {
            CompositionService = catalog.CompositionService;
            ExportProvider = catalog.ExportProvider;
            MainThread = Thread.CurrentThread;
        }

        /// <summary>
        /// Called via reflection from CoreShell.TryCreateTestInstance
        /// in test scenarios
        /// </summary>
        public static void Create() {
            lock (_lock) {
                // Called via reflection in test cases. Creates instance
                // of the test shell that editor code can access during
                // test run.
                _instance = new TestEditorShell(EditorTestCompositionCatalog.Current);
                EditorShell.Current = _instance;
            }
        }

        #region ICompositionCatalog
        public ICompositionService CompositionService { get; private set; }
        public ExportProvider ExportProvider { get; private set; }
        #endregion

        #region ICoreShell
        public Thread MainThread { get; set; }
        public void ShowErrorMessage(string msg) { }

        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        public MessageButtons ShowMessage(string message, MessageButtons buttons) {
            return MessageButtons.OK;
        }
        public T GetGlobalService<T>(Type type = null) where T : class {
            throw new NotImplementedException();
        }

        public void DoIdle() {
            if (Idle != null) {
                Idle(null, EventArgs.Empty);
            }
            DoEvents();
        }

        public void DispatchOnUIThread(Action action) {
            if (!MainThread.IsBackground) {
                var disp = Dispatcher.FromThread(MainThread);
                if (disp != null) {
                    disp.BeginInvoke(action, DispatcherPriority.Normal);
                    return;
                }
            }
            action();
        }

        public void DoEvents() {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrame(object f) {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }

        public int LocaleId => 1033;
        public bool IsUnitTestEnvironment { get; set; } = true;
        public bool IsUITestEnvironment { get; set; } = false;

        public event EventHandler<EventArgs> Idle;
#pragma warning disable 0067
        public event EventHandler<EventArgs> Terminating;
#pragma warning restore 0067
        #endregion

        #region IEditorShell
        public ICommandTarget TranslateCommandTarget(ITextView textView, object commandTarget) {
            return commandTarget as ICommandTarget;
        }

        public object TranslateToHostCommandTarget(ITextView textView, object commandTarget) {
            return commandTarget;
        }

        public ICompoundUndoAction CreateCompoundAction(ITextView textView, ITextBuffer textBuffer) {
            return new CompoundUndoAction(textView, textBuffer, addRollbackOnCancel: false);
        }
        #endregion
    }
}
