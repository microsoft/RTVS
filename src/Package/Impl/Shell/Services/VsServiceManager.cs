// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Testing;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.R.Packages.R;
using VsPackage = Microsoft.VisualStudio.Shell.Package;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsServiceManager : ServiceManager {
        private readonly ICoreShell _shell;
        private Lazy<IComponentModel> _componentModel = new Lazy<IComponentModel>(() => (IComponentModel)VsPackage.GetGlobalService(typeof(SComponentModel)));
        private ICompositionService _compositionService;
        private ExportProvider _exportProvider;

        public VsServiceManager(ICoreShell shell) {
            _shell = shell;
        }

        private IComponentModel ComponentModel => _componentModel.Value;

        internal ICompositionService CompositionService {
            get {
                _compositionService = _compositionService ?? ComponentModel.DefaultCompositionService;
                return _compositionService;
            }
            set {
                Debug.Assert(_compositionService == null);
                _compositionService = value;
            }
        }

        internal ExportProvider ExportProvider {
            get {
                _exportProvider = _exportProvider ?? ComponentModel?.DefaultExportProvider;
                return _exportProvider;
            }
            set {
                Debug.Assert(_exportProvider == null);
                _exportProvider = value;
            }
        }

        #region IServiceContainer
        public override T GetService<T>(Type type = null) {
            type = type ?? typeof(T);

            if (type == typeof(ICoreShell)) {
                return _shell as T;
            }
            if (type == typeof(ExportProvider)) {
                return ExportProvider as T;
            }
            if (type == typeof(ICompositionService)) {
                return CompositionService as T;
            }
            if (type == typeof(ICompositionCatalog)) {
                return (T) (ICompositionCatalog) new CompositionCatalog(CompositionService, ExportProvider);
            }

            // First try internal services
            var service = base.GetService<T>(type);
            if (service == null) {
                // First try MEF
                service = ExportProvider.GetExportedValueOrDefault<T>();
                if (service == null) {
                    // Now try VS services. Only allowed on UI thread.
                    if (TestEnvironment.Current != null) {
                        service = RPackage.Current != null ? RPackage.Current.GetService(type) as T : null;
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
