// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VsPackage = Microsoft.VisualStudio.Shell.Package;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed partial class VsAppShell {
        private IVsShell _vsShell;
        private uint _vsShellEventsCookie;

        public static void EnsureInitialized() {
            var instance = GetInstance();
            if (instance.MainThread == null) {
                instance.Initialize();
            }
        }

        public static void Terminate() {
            _instance?.Dispose();
        }

        private void Initialize() {
            MainThread = Thread.CurrentThread;
            MainThreadDispatcher = Dispatcher.FromThread(MainThread);

            CheckVsStarted();

            ConfigureServices();
            ConfigureIdleSource();
        }

        private void CheckVsStarted() {
            _vsShell = (IVsShell)VsPackage.GetGlobalService(typeof(SVsShell));
            object value;
            _vsShell.GetProperty((int)__VSSPROPID4.VSSPROPID_ShellInitialized, out value);
            if (value is bool) {
                if ((bool)value) {
                    Started?.Invoke(this, EventArgs.Empty);
                } else {
                    _vsShell.AdviseShellPropertyChanges(this, out _vsShellEventsCookie);
                }
            }
        }

        /// <summary>
        private static VsAppShell GetInstance() {
            if (_instance != null) {
                return _instance;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            var componentModel = (IComponentModel)VsPackage.GetGlobalService(typeof(SComponentModel));
            var instance = (VsAppShell)componentModel.DefaultExportProvider.GetExportedValue<ICoreShell>();

            return Interlocked.CompareExchange(ref _instance, instance, null) ?? instance;
        }

        #region IDisposable
        public void Dispose() {
            DisconnectFromShellEvents();
            _services?.Dispose();
        }
        #endregion

        public int OnShellPropertyChange(int propid, object var) {
            if (propid == (int)__VSSPROPID4.VSSPROPID_ShellInitialized) {
                Started?.Invoke(this, EventArgs.Empty);
            }
            return VSConstants.S_OK;
        }

        private void DisconnectFromShellEvents() {
            if (_vsShell != null) {
                if (_vsShellEventsCookie != 0) {
                    _vsShell.UnadviseShellPropertyChanges(_vsShellEventsCookie);
                    _vsShellEventsCookie = 0;
                }
            }
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
