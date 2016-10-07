// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Sql.Publish;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    [Export(typeof(IDacPackageServicesProvider))]
    internal sealed class DacPackageServicesProvider: IDacPackageServicesProvider {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public DacPackageServicesProvider(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public IDacPackageServices GetDacPackageServices() {
            if(SqlTools.CheckInstalled(_coreShell)) {
                return _coreShell.ExportProvider.GetExportedValue<IDacPackageServices>();
            }
            return null;
        }
    }
}
