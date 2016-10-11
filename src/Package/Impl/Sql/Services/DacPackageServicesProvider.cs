// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Sql.Publish;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    [Export(typeof(IDacPackageServicesProvider))]
    internal sealed class DacPackageServicesProvider: IDacPackageServicesProvider {
        private readonly ICoreShell _coreShell;
        private readonly IFileExtensionRegistryService _fers;
        private readonly IContentTypeRegistryService _ctrs;

        [ImportingConstructor]
        public DacPackageServicesProvider(ICoreShell coreShell, IFileExtensionRegistryService fers, IContentTypeRegistryService ctrs) {
            _coreShell = coreShell;
            _fers = fers;
            _ctrs = ctrs;
        }

        public IDacPackageServices GetDacPackageServices() {
            if(IsSqlToolsInstalled()) {
                return _coreShell.ExportProvider.GetExportedValueOrDefault<IDacPackageServices>();
            }
            return null;
        }

        private bool IsSqlToolsInstalled() {
            bool installed = false;

            // Try modern registration first
            var ct = _fers.GetContentTypeForExtension(".sql");
            if (ct != null && ct != _ctrs.UnknownContentType) {
                installed = true;
            } else {
                // Fall back to IVs* type of registration
                ct = _ctrs.GetContentType("SQL Server Tools");
                installed = ct != null && ct != _ctrs.UnknownContentType;
            }
            return installed;
        }
    }
}
