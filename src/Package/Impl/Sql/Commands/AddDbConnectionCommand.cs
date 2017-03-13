// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.Common.Core;
using Microsoft.R.Components.Sql;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal sealed class AddDbConnectionCommand : ConfigurationSettingCommand {
        private readonly IDbConnectionService _dbcs;

        public AddDbConnectionCommand(IDbConnectionService dbcs, IProjectSystemServices pss,
                IProjectConfigurationSettingsProvider pcsp, IRInteractiveWorkflow workflow) :
            base(RPackageCommandId.icmdAddDatabaseConnection, "dbConnection", pss, pcsp, workflow) {
            _dbcs = dbcs;
        }

        protected override void Handle() {
            var connString = _dbcs.EditConnectionString(null);
            if (!string.IsNullOrWhiteSpace(connString)) {
                SaveSetting(connString).DoNotWait();
            }
        }
    }
}
