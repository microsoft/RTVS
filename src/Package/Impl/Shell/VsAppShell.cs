// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Settings;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.Threading;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Host;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Interop;
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

        private readonly IRSettings _settings;
        private readonly ICoreServices _coreServices;

        private IdleTimeSource _idleTimeSource;
        private IWritableSettingsStorage _settingStorage;

        [ImportingConstructor]
        public VsAppShell(ITelemetryService telemetryService
            , IRSettings settings
            , ICoreServices coreServices
            , IApplicationConstants appConstants) {

            _coreServices = coreServices;
            AppConstants = appConstants;
            ProgressDialog = new VsProgressDialog(this);
            FileDialog = new VsFileDialog(this);

            _settings = settings;
            _settings.PropertyChanged += OnSettingsChanged;
        }

        public static void EnsureInitialized() {
            ThreadHelper.ThrowIfNotOnUIThread();
            var instance = GetInstance();
            if (instance.MainThread == null) {
                instance.Initialize();
            }
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(IRSettings.LogVerbosity)) {
                // Only set it once per session
                _settings.PropertyChanged -= OnSettingsChanged;
                _coreServices.LoggingServices.Permissions.CurrentVerbosity = _settings.LogVerbosity;
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
        public MessageButtons ShowMessage(string message, MessageButtons buttons, MessageType messageType = MessageType.Information) {
            var shell = GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            int result;

            var oleButtons = GetOleButtonFlags(buttons);
            OLEMSGICON oleIcon;

            switch (messageType) {
                case MessageType.Information:
                    oleIcon = buttons == MessageButtons.OK ? OLEMSGICON.OLEMSGICON_INFO : OLEMSGICON.OLEMSGICON_QUERY;
                    break;
                case MessageType.Warning:
                    oleIcon = OLEMSGICON.OLEMSGICON_WARNING;
                    break;
                default:
                    oleIcon = OLEMSGICON.OLEMSGICON_CRITICAL;
                    break;
            }

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

        public void UpdateCommandStatus(bool immediate) {
            DispatchOnUIThread(() => {
                var uiShell = GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                uiShell.UpdateCommandUI(immediate ? 1 : 0);
            });
        }

        public bool IsUnitTestEnvironment { get; set; }

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
        public void Post(Action action) {
            var awaiter = ThreadHelper.JoinableTaskFactory
                .SwitchToMainThreadAsync()
                .GetAwaiter();

            awaiter.OnCompleted(action);
        }

        #endregion

        public void Dispose() {
            _coreServices.LoggingServices.Dispose();
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

        public ICoreServices Services => _coreServices;
        public IApplicationConstants AppConstants { get; }
        public IProgressDialog ProgressDialog { get; }
        public IFileDialog FileDialog { get; }
    }
}
