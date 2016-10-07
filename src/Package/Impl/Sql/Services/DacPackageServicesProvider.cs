// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Sql.Publish;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class DacPackageServicesProvider: IDacPackageServicesProvider {
        public IDacPackageServices GetDacPackageServices(ICoreShell coreShell) {
            if(SqlTools.CheckInstalled(coreShell, showMessage: false)) {
                return coreShell.ExportProvider.GetExportedValue<IDacPackageServices>();
            }
            return null;
        }
    }
}
