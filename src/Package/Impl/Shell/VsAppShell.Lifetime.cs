// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.R.Package.Wpf;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VsPackage = Microsoft.VisualStudio.Shell.Package;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed partial class VsAppShell {
        private IVsShell _vsShell;
        private uint _vsShellEventsCookie;

        public static void EnsureInitialized() {
            var instance = GetInstance();
            if (instance._vsShell == null) {
                instance.Initialize();
            }
        }

        public static void Terminate() {
            _instance?.Dispose();
        }

        private void Initialize() {
            VsWpfOverrides.Apply();

            ConfigurePackageServices();
            ConfigureIdleSource();
            CheckVsStarted();
        }

        private void CheckVsStarted() {
            _vsShell = (IVsShell)VsPackage.GetGlobalService(typeof(SVsShell));
            _vsShell.GetProperty((int)__VSSPROPID4.VSSPROPID_ShellInitialized, out var value);
            if (value is bool) {
                if ((bool)value) {
                    _application.FireStarted();
                } else {
                    _vsShell.AdviseShellPropertyChanges(this, out _vsShellEventsCookie);
                }
            }
        }

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
                _application.FireStarted();
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
