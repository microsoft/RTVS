using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Host;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public class VsEditorShell : IEditorShell, IDisposable, IIdleTimeService {
        [ImportMany]
        private IEnumerable<Lazy<IAppShellInitialization>> ShellInitializers { get; set; }
        private IdleTimeSource _idleTimeSource;

        public VsEditorShell() {
            ThreadHelper.ThrowIfNotOnUIThread("VsEditorShell.Current");
            MainThread = Thread.CurrentThread;

            IComponentModel componentModel = RPackage.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            CompositionService = componentModel.DefaultCompositionService;
            ExportProvider = componentModel.DefaultExportProvider;

            _idleTimeSource = new IdleTimeSource();
            _idleTimeSource.OnIdle += OnIdle;
            _idleTimeSource.OnTerminateApp += OnTerminateApp;

            CompositionService.SatisfyImportsOnce(this);

            foreach (var init in ShellInitializers) {
                init.Value.SetShell(this);
            }
        }

        #region IApplicationShell
        /// <summary>
        /// Retrieves Visual Studio global service from global VS service provider.
        /// This method is not thread safe and should not be called from async methods.
        /// </summary>
        /// <typeparam name="T">Service interface type such as IVsUiShell</typeparam>
        /// <param name="type">Service type if different from T, such as typeof(SVSUiShell)</param>
        /// <returns>Service instance of null if not found.</returns>
        public T GetGlobalService<T>(Type type = null) where T : class {
            if (IsUnitTestEnvironment) {
                System.IServiceProvider sp = RPackage.Current;
                return sp.GetService(type ?? typeof(T)) as T;
            }

            return RPackage.GetGlobalService(type ?? typeof(T)) as T;
        }

        /// <summary>
        /// Application composition service
        /// </summary>
        public ICompositionService CompositionService { get; private set; }

        /// <summary>
        /// Application export provider
        /// </summary>
        public ExportProvider ExportProvider { get; private set; }

        /// <summary>
        /// Provides a way to execute action on UI thread while
        /// UI thread is waiting for the completion of the action.
        /// May be implemented using ThreadHelper in VS or via
        /// SynchronizationContext in all-managed application.
        /// 
        /// This can be blocking or non blocking dispatch, preferrably
        /// non blocking
        /// </summary>
        /// <param name="action">Action to execute</param>
        public void DispatchOnUIThread(Action action) {
            if (MainThread != null) {
                var dispatcher = Dispatcher.FromThread(MainThread);
                Debug.Assert(dispatcher != null);

                if (dispatcher != null && !dispatcher.HasShutdownStarted) {
                    dispatcher.BeginInvoke(action, DispatcherPriority.Normal);
                }
            } else {
                Debug.Assert(false);
                ThreadHelper.Generic.BeginInvoke(DispatcherPriority.Normal, () => action());
            }
        }

        /// <summary>
        /// Provides access to the application main thread, so users can know if the task they are trying
        /// to execute is executing from the right thread.
        /// </summary>
        public Thread MainThread { get; set; }

        /// <summary>
        /// Fires when host application enters idle state.
        /// </summary>
        public event EventHandler<EventArgs> Idle;

        /// <summary>
        /// Fires when host application is terminating
        /// </summary>
        public event EventHandler<EventArgs> Terminating;

        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        public void ShowErrorMessage(string message) {
            var shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            int result;

            shell.ShowMessageBox(0, Guid.Empty, null, message, null, 0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 0, out result);
        }

        /// <summary>
        /// Displays question in a host-specific UI
        /// </summary>
        public MessageButtons ShowMessage(string message, MessageButtons buttons) {
            var shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            int result;

            var oleButtons = GetOleButtonFlags(buttons);
            shell.ShowMessageBox(0, Guid.Empty, null, message, null, 0,
                oleButtons, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_QUERY, 0, out result);

            switch (result) {
                case NativeMethods.IDYES:
                    return MessageButtons.Yes;
                case NativeMethods.IDNO:
                    return MessageButtons.No;
                case NativeMethods.IDCANCEL:
                    return MessageButtons.Cancel;
            }
            return MessageButtons.OK;
        }

        public bool IsUnitTestEnvironment { get; set; }

        public bool IsUITestEnvironment { get; set; }
        #endregion

        #region IEditorShell 
        /// <summary>
        /// Provides shim that implements ICommandTarget over 
        /// application-specific command target. For example, 
        /// Visual Studio is using IOleCommandTarget.
        /// </summary>
        /// <param name="commandTarget">Command target</param>
        /// <returns>Web components compatible command target</returns>
        public ICommandTarget TranslateCommandTarget(ITextView textView, object commandTarget) {
            var managedCommandTarget = commandTarget as ICommandTarget;
            if (managedCommandTarget != null)
                return managedCommandTarget;

            var oleCommandTarget = commandTarget as IOleCommandTarget;
            if (oleCommandTarget != null)
                return new OleToCommandTargetShim(textView, oleCommandTarget);

            Debug.Fail("Unknown command taget type");
            return null;

        }

        public object TranslateToHostCommandTarget(ITextView textView, object commandTarget) {
            var oleToCommandTargetShim = commandTarget as OleToCommandTargetShim;
            if (oleToCommandTargetShim != null)
                return oleToCommandTargetShim.OleTarget;

            var managedCommandTarget = commandTarget as ICommandTarget;
            if (managedCommandTarget != null)
                return new CommandTargetToOleShim(textView, managedCommandTarget);

            var oleCommandTarget = commandTarget as IOleCommandTarget;
            if (oleCommandTarget != null)
                return oleCommandTarget;

            Debug.Fail("Unknown command taget type");
            return null;
        }

        /// <summary>
        /// Creates compound undo action
        /// </summary>
        /// <param name="textView">Text view</param>
        /// <param name="textBuffer">Text buffer</param>
        /// <returns>Undo action instance</returns>
        public ICompoundUndoAction CreateCompoundAction(ITextView textView, ITextBuffer textBuffer) {
            return new CompoundUndoAction(textView, textBuffer, addRollbackOnCancel: true);
        }
        #endregion

        void OnIdle(object sender, EventArgs args) {
            DoIdle();
        }

        private void OnTerminateApp(object sender, EventArgs args) {
            Dispose();
        }

        #region IIdleTimeService
        public void DoIdle() {
            if (Idle != null)
                Idle(this, EventArgs.Empty);
        }
        #endregion

        /// <summary>
        /// Displays help on the specified topic
        /// </summary>
        public bool ShowHelp(string topicName) {
            return true;
        }

        /// <summary>
        /// Returns host locale ID
        /// </summary>
        public int LocaleId {
            get {
                IUIHostLocale hostLocale = GetGlobalService<IUIHostLocale>();
                uint lcid;

                if (hostLocale != null && hostLocale.GetUILocale(out lcid) == VSConstants.S_OK) {
                    return (int)lcid;
                }
                return 0;
            }
        }

        /// <summary>
        /// Returns path to application-specific user folder, such as VisualStudio\11.0
        /// </summary>
        public string UserFolder {
            get {
                var settingsManager = new ShellSettingsManager(RPackage.Current);
                return settingsManager.GetApplicationDataFolder(ApplicationDataFolder.RoamingSettings);
            }
        }

        #region IDisposable
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_idleTimeSource != null) {
                    _idleTimeSource.OnIdle -= OnIdle;
                    _idleTimeSource.OnTerminateApp -= OnTerminateApp;
                    _idleTimeSource.Dispose();
                    _idleTimeSource = null;

                    if (Terminating != null) {
                        Terminating(this, EventArgs.Empty);
                    }
                }
            }
        }
        #endregion

        private OLEMSGBUTTON GetOleButtonFlags(MessageButtons buttons) {
            switch (buttons) {
                case MessageButtons.YesNoCancel:
                    return OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL;
                case MessageButtons.YesNo:
                    return OLEMSGBUTTON.OLEMSGBUTTON_YESNO;
                case MessageButtons.OKCancel:
                    return OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL;
            }
            return OLEMSGBUTTON.OLEMSGBUTTON_OK;
        }
    }
}
