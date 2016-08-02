// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using Microsoft.Common.Core.Shell;
using Microsoft.SqlServer.Dac;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class DacPackageServices : IDacPackageServices {
        public IDacPacBuilder GetBuilder(ICoreShell coreShell) {
            return new DacPacBuilder(coreShell);
        }

        public void Deploy(DacPackage package, string connectionString, string databaseName) {
            var services = new DacServices(connectionString);
            services.Deploy(package, databaseName, upgradeExisting: true);
        }

        public DacPackage Load(string dacpacPath) {
            return DacPackage.Load(dacpacPath);
        }
    }
}
