// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.Sql.Publish;
using Microsoft.SqlServer.Dac;

namespace Microsoft.VisualStudio.R.Sql.Publish {
    internal sealed class DacPackageImpl: IDacPackage {
        private readonly DacPackage _dacpac;

        public DacPackageImpl(string dacpacPath) {
            _dacpac = DacPackage.Load(dacpacPath);
        }

        public void Deploy(string connectionString, string databaseName) {
            var services = new DacServices(connectionString);
            var options = new DacDeployOptions {
                ScriptDatabaseOptions = false,
                BlockOnPossibleDataLoss = true
            };
            services.Deploy(_dacpac, databaseName, upgradeExisting: true, options: options);
        }

        public void Dispose() {
            _dacpac.Dispose();
        }
    }
}
