// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.R.Host.Client;
using Microsoft.Common.Core;
using Microsoft.R.Components.Sql;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.Sql {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(Constants.RtvsProjectCapability)]
    internal sealed class AddDbConnectionCommand : ConfigurationSettingCommand {
        private readonly IDbConnectionService _dbcs;

        public AddDbConnectionCommand(IDbConnectionService dbcs, IProjectSystemServices pss,
                IProjectConfigurationSettingsProvider pcsp, IRSession session) :
            base(RPackageCommandId.icmdAddDatabaseConnection, "dbConnection", pss, pcsp, session) {
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
