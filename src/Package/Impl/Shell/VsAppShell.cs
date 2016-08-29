// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Wpf.Threading;
using Microsoft.Languages.Core.Settings;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Host;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Extensions;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using static System.FormattableString;
using IServiceProvider = System.IServiceProvider;
using VsPackage = Microsoft.VisualStudio.Shell.Package;

namespace Microsoft.VisualStudio.R.Package.Shell {
    /// <summary>
    /// Application shell provides access to services
    /// such as composition container, export provider, global VS IDE
    /// services and so on.
    /// </summary>
    [Export(typeof(ICoreShell))]
    [Export(typeof(IEditorShell))]
    [Export(typeof(IApplicationShell))]
    public sealed class VsAppShell : IApplicationShell, IMainThread, IIdleTimeService, IDisposable {
        private static VsAppShell _instance;
        private static IApplicationShell _testShell;
        private IdleTimeSource _idleTimeSource;
        private IWritableSettingsStorage _settingStorage;

        public static void EnsureInitialized() {
            ThreadHelper.ThrowIfNotOnUIThread();
            var instance = GetInstance();
            if (instance.MainThread == null) {
                instance.Initialize();
            }
        }

        private void Initialize() {
            MainThread = Thread.CurrentThread;
            MainThreadDispatcher = Dispatcher.FromThread(MainThread);

            _idleTimeSource = new IdleTimeSource();
            _idleTimeSource.OnIdle += OnIdle;
            _idleTimeSource.OnTerminateApp += OnTerminateApp;

            EditorShell.Current = this;
        }

        /// <summary>
        /// Current application shell instance. Provides access to services
        /// such as composition container, export provider, global VS IDE
        /// services and so on.
        /// </summary>
        public static IApplicationShell Current {
            get {
                if (_testShell == null && _instance == null) {
                    // Try test environment
                    CoreShell.TryCreateTestInstance("Microsoft.VisualStudio.R.Package.Test.dll", "TestVsAppShell");
                }

                return _testShell ?? GetInstance();
            }
            internal set {
                // Normally only called in test cases when package
                // is not loaded and hence shell is not initialized.
                // In this case test code provides replacement shell
                // which we then pass to any other shell-type objects
                // to use.
                if (_instance != null) {
                    throw new InvalidOperationException("Cannot set test shell when real one is already there.");
                }
                if (_testShell == null) {
                    _testShell = value;
                }
            }
        }

        private static VsAppShell GetInstance() {
            if (_instance != null) {
                return _instance;
            }

            var componentModel = (IComponentModel)VsPackage.GetGlobalService(typeof(SComponentModel));
            var instance = (VsAppShell)componentModel.DefaultExportProvider.GetExportedValue<IApplicationShell>();
            instance.CompositionService = componentModel.DefaultCompositionService;
            instance.ExportProvider = componentModel.DefaultExportProvider;
            return Interlocked.CompareExchange(ref _instance, instance, null) ?? instance;
        }

        #region ICompositionCatalog
        /// <summary>
        /// Application composition service
        /// </summary>
        public ICompositionService CompositionService { get; private set; }

        /// <summary>
        /// Application export provider
        /// </summary>
        public ExportProvider ExportProvider { get; private set; }
        #endregion

        #region ICoreShell
        /// <summary>
        /// Retrieves Visual Studio global service from global VS service provider.
        /// This method is not thread safe and should not be called from async methods.
        /// </summary>
        /// <typeparam name="T">Service interface type such as IVsUiShell</typeparam>
        /// <param name="type">Service type if different from T, such as typeof(SVSUiShell)</param>
        /// <returns>Service instance of null if not found.</returns>
        public T GetGlobalService<T>(Type type = null) where T : class {
            this.AssertIsOnMainThread();
            if (IsUnitTestEnvironment) {
                System.IServiceProvider sp = RPackage.Current;
                return sp.GetService(type ?? typeof(T)) as T;
            }

            return VsPackage.GetGlobalService(type ?? typeof(T)) as T;
        }

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
                Debug.Assert(MainThreadDispatcher != null);

                if (MainThreadDispatcher != null && !MainThreadDispatcher.HasShutdownStarted) {
                    MainThreadDispatcher.BeginInvoke(action, DispatcherPriority.Normal);
                }
            } else {
                Debug.Assert(false);
                ThreadHelper.Generic.BeginInvoke(DispatcherPriority.Normal, () => action());
            }
        }


        private Dispatcher MainThreadDispatcher { get; set; }

        /// <summary>
        /// Provides access to the application main thread, so users can know if the task they are trying
        /// to execute is executing from the right thread.
        /// </summary>
        public Thread MainThread { get; private set; }

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
            var shell = GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            int result;

            shell.ShowMessageBox(0, Guid.Empty, null, message, null, 0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 0, out result);
        }

        public void ShowContextMenu(CommandID commandId, int x, int y, object commandTarget = null) {
            if (commandTarget == null) {
                var package = EnsurePackageLoaded(RGuidList.RPackageGuid);
                if (package != null) {
                    var menuService = (IMenuCommandService)((IServiceProvider)package).GetService(typeof(IMenuCommandService));
                    menuService.ShowContextMenu(commandId, x, y);
                }
            } else {
                var target = commandTarget as ICommandTarget;
                if (target == null) {
                    throw new ArgumentException(Invariant($"{nameof(commandTarget)} must implement ICommandTarget"));
                }
                var shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                var pts = new POINTS[1];
                pts[0].x = (short)x;
                pts[0].y = (short)y;
                shell.ShowContextMenu(0, commandId.Guid, commandId.ID, pts, new CommandTargetToOleShim(null, target));
            }
        }

        /// <summary>
        /// Displays question in a host-specific UI
        /// </summary>
        public MessageButtons ShowMessage(string message, MessageButtons buttons) {
            var shell = GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            int result;

            var oleButtons = GetOleButtonFlags(buttons);
            var oleIcon = oleButtons == OLEMSGBUTTON.OLEMSGBUTTON_OK ? OLEMSGICON.OLEMSGICON_WARNING : OLEMSGICON.OLEMSGICON_QUERY;

            shell.ShowMessageBox(0, Guid.Empty, null, message, null, 0,
                oleButtons, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, oleIcon, 0, out result);

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

        public string SaveFileIfDirty(string fullPath) =>
            new RunningDocumentTable(RPackage.Current).SaveFileIfDirty(fullPath);

        public string ShowOpenFileDialog(string filter, string initialPath = null, string title = null) {
            return BrowseForFileOpen(IntPtr.Zero, filter, initialPath, title);
        }

        public string ShowSaveFileDialog(string filter, string initialPath = null, string title = null) {
            return BrowseForFileSave(IntPtr.Zero, filter, initialPath, title);
        }

        public void UpdateCommandStatus(bool immediate) {
            DispatchOnUIThread(() => {
                var uiShell = GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                uiShell.UpdateCommandUI(immediate ? 1 : 0);
            });
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

        public ITelemetryService TelemetryService => RtvsTelemetry.Current.TelemetryService;

        public bool IsUnitTestEnvironment { get; set; }

        public IntPtr ApplicationWindowHandle {
            get {
                var uiShell = GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                IntPtr handle;
                uiShell.GetDialogOwnerHwnd(out handle);
                return handle;
            }
        }

        #endregion

        #region IIdleTimeService
        public void DoIdle() {
            Idle?.Invoke(this, EventArgs.Empty);
        }
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
            return new CompoundUndoAction(textView, this, addRollbackOnCancel: true);
        }
        #endregion

        #region IApplicationShell
        public string BrowseForFileOpen(IntPtr owner, string filter, string initialPath = null, string title = null) {
            IVsUIShell uiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            if (uiShell == null) {
                return null;
            }

            if (owner == IntPtr.Zero) {
                ErrorHandler.ThrowOnFailure(uiShell.GetDialogOwnerHwnd(out owner));
            }

            VSOPENFILENAMEW[] openInfo = new VSOPENFILENAMEW[1];
            openInfo[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSOPENFILENAMEW));
            openInfo[0].pwzFilter = filter.Replace('|', '\0') + "\0";
            openInfo[0].hwndOwner = owner;
            openInfo[0].pwzDlgTitle = title;
            openInfo[0].nMaxFileName = 260;
            var pFileName = Marshal.AllocCoTaskMem(520);
            openInfo[0].pwzFileName = pFileName;
            openInfo[0].pwzInitialDir = Path.GetDirectoryName(initialPath);
            var nameArray = (Path.GetFileName(initialPath) + "\0").ToCharArray();
            Marshal.Copy(nameArray, 0, pFileName, nameArray.Length);

            try {
                int hr = uiShell.GetOpenFileNameViaDlg(openInfo);
                if (hr == VSConstants.OLE_E_PROMPTSAVECANCELLED) {
                    return null;
                }
                ErrorHandler.ThrowOnFailure(hr);
                return Marshal.PtrToStringAuto(openInfo[0].pwzFileName);
            } finally {
                if (pFileName != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(pFileName);
                }
            }
        }

        public string BrowseForFileSave(IntPtr owner, string filter, string initialPath = null, string title = null) {
            if (string.IsNullOrEmpty(initialPath)) {
                initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + Path.DirectorySeparatorChar;
            }

            IVsUIShell uiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            if (null == uiShell) {
                return null;
            }

            if (owner == IntPtr.Zero) {
                ErrorHandler.ThrowOnFailure(uiShell.GetDialogOwnerHwnd(out owner));
            }

            VSSAVEFILENAMEW[] saveInfo = new VSSAVEFILENAMEW[1];
            saveInfo[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSSAVEFILENAMEW));
            saveInfo[0].dwFlags = 0x00000002; // OFN_OVERWRITEPROMPT
            saveInfo[0].pwzFilter = filter.Replace('|', '\0') + "\0";
            saveInfo[0].hwndOwner = owner;
            saveInfo[0].pwzDlgTitle = title;
            saveInfo[0].nMaxFileName = 260;
            var pFileName = Marshal.AllocCoTaskMem(520);
            saveInfo[0].pwzFileName = pFileName;
            saveInfo[0].pwzInitialDir = Path.GetDirectoryName(initialPath);
            var nameArray = (Path.GetFileName(initialPath) + "\0").ToCharArray();
            Marshal.Copy(nameArray, 0, pFileName, nameArray.Length);
            try {
                int hr = uiShell.GetSaveFileNameViaDlg(saveInfo);
                if (hr == VSConstants.OLE_E_PROMPTSAVECANCELLED) {
                    return null;
                }
                ErrorHandler.ThrowOnFailure(hr);
                return Marshal.PtrToStringAuto(saveInfo[0].pwzFileName);
            } finally {
                if (pFileName != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(pFileName);
                }
            }
        }

        public IWritableSettingsStorage SettingsStorage {
            get {
                if (_settingStorage == null) {
                    var ctrs = ExportProvider.GetExportedValue<IContentTypeRegistryService>();
                    var contentType = ctrs.GetContentType(RContentTypeDefinition.ContentType);
                    _settingStorage = ComponentLocatorForOrderedContentType<IWritableSettingsStorage>
                                            .FindFirstOrderedComponent(CompositionService, contentType);
                }
                return _settingStorage;
            }
        }

        #endregion

        #region
        public int ThreadId => MainThread.ManagedThreadId;
        public void Post(Action action) => ThreadHelper.JoinableTaskFactory
                    .SwitchToMainThreadAsync()
                    .GetAwaiter()
                    .OnCompleted(action);
        #endregion

        public void Dispose() {
        }

        void OnIdle(object sender, EventArgs args) {
            DoIdle();
        }

        private void OnTerminateApp(object sender, EventArgs args) {
            Terminating?.Invoke(null, EventArgs.Empty);
            Dispose();
        }

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

        public static IVsPackage EnsurePackageLoaded(Guid guidPackage) {
            var shell = (IVsShell)VsPackage.GetGlobalService(typeof(IVsShell));
            var guid = guidPackage;
            IVsPackage package;
            int hr = ErrorHandler.ThrowOnFailure(shell.IsPackageLoaded(ref guid, out package), VSConstants.E_FAIL);
            guid = guidPackage;
            if (hr != VSConstants.S_OK) {
                ErrorHandler.ThrowOnFailure(shell.LoadPackage(ref guid, out package), VSConstants.E_FAIL);
            }

            return package;
        }
    }
}
