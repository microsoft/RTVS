// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.R.Packages.R;
using VsPackage = Microsoft.VisualStudio.Shell.Package;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsServiceManager : ServiceManager {
        private readonly ICoreShell _shell;
        private readonly ICompositionService _compositionService;
        private readonly ExportProvider _exportProvider;

        public VsServiceManager(ICoreShell shell) {
            _shell = shell;
            var componentModel = (IComponentModel)VsPackage.GetGlobalService(typeof(SComponentModel));
            _compositionService = componentModel.DefaultCompositionService;
            _exportProvider = componentModel.DefaultExportProvider;
        }

        #region IServiceContainer
        public override T GetService<T>(Type type = null) {
            type = type ?? typeof(T);

            if(type == typeof(ExportProvider)) {
                return _exportProvider as T;
            }
            if (type == typeof(ICompositionService)) {
                return _compositionService as T;
            }
            if (type == typeof(ICompositionCatalog)) {
                return (T) (ICompositionCatalog) new CompositionCatalog(_compositionService, _exportProvider);
            }

            // First try internal services
            var service = base.GetService<T>(type);
            if (service == null) {
                // First try MEF
                service = _exportProvider.GetExportedValueOrDefault<T>();
                if (service == null) {
                    // Now try VS services. Only allowed on UI thread.
                    _shell.AssertIsOnMainThread();
                    if (_shell.IsUnitTestEnvironment) {
                        service = RPackage.Current.GetService(type) as T;
                    } else {
                        service = VsPackage.GetGlobalService(type) as T;
                    }
                }
            }
            return service;
        }
        #endregion
    }
}
