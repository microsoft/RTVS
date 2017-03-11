// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using VsPackage = Microsoft.VisualStudio.Shell.Package;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsServiceManager: ServiceManager {
        private readonly IApplicationShell _appShell;

        public VsServiceManager(IApplicationShell appShell) {
            _appShell = appShell;
        }

        #region IServiceContainer
        public override T GetService<T>(Type type = null) {
            // First try internal services
            var service = base.GetService<T>(type);
            if (service == null) {
                // First try MEF
                service = _appShell.ExportProvider.GetExportedValue<T>();
                if (service == null) {
                    // Now try VS services. Only allowed on UI thread.
                    _appShell.AssertIsOnMainThread();
                    if (_appShell.IsUnitTestEnvironment) {
                        service = RPackage.Current.GetService(type ?? typeof(T)) as T;
                    } else {
                        service = VsPackage.GetGlobalService(type ?? typeof(T)) as T;
                    }
                }
            }
            return service;
        }
        #endregion
    }
}
