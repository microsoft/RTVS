// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.R.Components.InteractiveWorkflow;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.Sql {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(Constants.RtvsProjectCapability)]
    internal sealed class AddDbConnectionCommand : ConfigurationSettingCommand {
        private readonly IDbConnectionService _dbcs;
         
        [ImportingConstructor]
        public AddDbConnectionCommand(ConfiguredProject configuredProject,
                ProjectProperties projectProperties,
                IDbConnectionService dbcs, IProjectConfigurationSettingsProvider pcsp,
                IRInteractiveWorkflowProvider workflowProvider) :
            base(RPackageCommandId.icmdAddDsn, "dbConnection", configuredProject, pcsp, workflowProvider) {
            _dbcs = dbcs;
        }

        public override async Task<bool> TryHandleCommandAsync() {
            var connString = _dbcs.EditConnectionString(null);
            if (!string.IsNullOrWhiteSpace(connString)) {
                await SaveSetting(connString);
                return true;
            }
            return false;
        }
    }
}
