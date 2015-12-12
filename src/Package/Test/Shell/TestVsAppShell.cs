using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    /// <summary>
    /// Replacement for VsAppShell in unit tests.
    /// Created via reflection by test code.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed class TestVsAppShell : IApplicationShell {
        private IServiceProvider _sp;
        private static TestVsAppShell _instance;

        private TestVsAppShell() {
            MainThread = Thread.CurrentThread;
            CompositionService = VsTestCompositionCatalog.Current.CompositionService;
            ExportProvider = VsTestCompositionCatalog.Current.ExportProvider;
            _sp = new TestServiceProvider();
        }

        public static void Create() {
            // Called via reflection in test cases. Creates instance
            // of the test shell that code can access during the test run.
            // other shell objects may choose to create their own
            // replacements. For example, interactive editor tests
            // need smaller MEF catalog which excludes certain 
            // VS-specific implementations.
            _instance = new TestVsAppShell();
            VsAppShell.Current = _instance;
            RToolsSettings.Current = new TestRToolsSettings();
        }

        #region ICompositionCatalog
        public ICompositionService CompositionService { get; private set; }
        public ExportProvider ExportProvider { get; private set; }
        #endregion

        #region IApplicationShell
        public string BrowseForFileOpen(IntPtr owner, string filter, string initialPath = null, string title = null) {
            return null;
        }
        public string BrowseForFileSave(IntPtr owner, string filter, string initialPath = null, string title = null) {
            return null;
        }
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

        #region ICoreShell
        public T GetGlobalService<T>(Type type = null) where T : class {
            return _sp.GetService(type ?? typeof(T)) as T;
        }

        public event EventHandler<EventArgs> Idle;
#pragma warning disable 0067
        public event EventHandler<EventArgs> Terminating;
#pragma warning restore 0067

        public Thread MainThread { get; set; }
        public void ShowErrorMessage(string msg) { }
        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        public MessageButtons ShowMessage(string message, MessageButtons buttons) {
            return MessageButtons.OK;
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
