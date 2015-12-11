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

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    [ExcludeFromCodeCoverage]
    class TestEditorShell : IEditorShell {
        private static object _lock = new object();

        private TestEditorShell(ICompositionService compositionService, ExportProvider exportProvider) : this() {
            CompositionService = compositionService;
            ExportProvider = exportProvider;
        }
        protected TestEditorShell() {
            MainThread = Thread.CurrentThread;
        }

        #region IEditorShell
        public event EventHandler<EventArgs> Idle;

#pragma warning disable 0067
        public event EventHandler<EventArgs> Terminating;
#pragma warning restore 0067

        public ICompositionService CompositionService { get; protected set; }
        public ExportProvider ExportProvider { get; protected set; }

        public ICommandTarget TranslateCommandTarget(ITextView textView, object commandTarget) {
            return commandTarget as ICommandTarget;
        }

        public object TranslateToHostCommandTarget(ITextView textView, object commandTarget) {
            return commandTarget;
        }

        public ICompoundUndoAction CreateCompoundAction(ITextView textView, ITextBuffer textBuffer) {
            return new CompoundUndoAction(textView, textBuffer, addRollbackOnCancel: false);
        }

        public Thread MainThread { get; set; }

        public void ShowErrorMessage(string msg) { }

        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        public MessageButtons ShowMessage(string message, MessageButtons buttons) {
            return MessageButtons.OK;
        }

        public virtual T GetGlobalService<T>(Type type = null) where T : class {
            throw new NotImplementedException();
        }

        public void DoIdle() {
            if (Idle != null) {
                Idle(null, EventArgs.Empty);
            }
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

        public int LocaleId => 1033;

        public bool IsUnitTestEnvironment { get; set; } = true;

        public bool IsUITestEnvironment { get; set; } = false;
        #endregion
    }
}
