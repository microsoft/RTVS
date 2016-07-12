// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.R.Components.InteractiveWorkflow;
using static System.FormattableString;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.Sql {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(Constants.RtvsProjectCapability)]
    internal sealed class AddDbConnectionCommand : IAsyncCommandGroupHandler {
        private const string _settingNameTemplate = "dbConnection";
        private readonly ConfiguredProject _configuredProject;
        private readonly ProjectProperties _projectProperties;
        private readonly IDbConnectionService _dbcs;
        private readonly IProjectConfigurationSettingsProvider _pcsp;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;

        [ImportingConstructor]
        public AddDbConnectionCommand(ConfiguredProject configuredProject, 
                ProjectProperties projectProperties, 
                IDbConnectionService dbcs, IProjectConfigurationSettingsProvider pcsp, 
                IRInteractiveWorkflowProvider workflowProvider) {
            _configuredProject = configuredProject;
            _projectProperties = projectProperties;
            _dbcs = dbcs;
            _pcsp = pcsp;
            _workflowProvider = workflowProvider;
        }

        public Task<CommandStatusResult> GetCommandStatusAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdAddDabaseConnection) {
                return Task.FromResult(new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported));
            }
            return Task.FromResult(CommandStatusResult.Unhandled);
        }

        public async Task<bool> TryHandleCommandAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdAddDabaseConnection) {
                var connString = _dbcs.EditConnectionString(null);
                if(!string.IsNullOrWhiteSpace(connString)) {
                    using (var access = await _pcsp.OpenProjectSettingsAccessAsync(_configuredProject)) {
                        var name = FindAvailableSettingName(access.Settings);
                        var s = new ConfigurationSetting(name, connString, ConfigurationSettingValueType.String);
                        s.EditorType = ConnectionStringEditor.ConnectionStringEditorName;
                        s.Category = ConnectionStringEditor.ConnectionStringEditorCategory;
                        s.Description = Resources.ConnectionStringDescription;
                        access.Settings.Add(s);

                        var session = _workflowProvider.GetOrCreate()?.RSession;
                        await session?.EvaluateAsync(Invariant($"{name} <- '{connString}'"), REvaluationKind.Mutating);
                    }
                }
                return true;
            }
            return false;
        }

        private string FindAvailableSettingName(ConfigurationSettingCollection settings) {
            var connections = new HashSet<string>();
            connections.AddRange(settings.Where(s => s.Name.StartsWithOrdinal(_settingNameTemplate)).Select(s => s.Name));
            if (connections.Count > 0) {
                for (int i = 1; i < 1000; i++) {
                    var candidate = _settingNameTemplate + i.ToString();
                    if (!connections.Contains(candidate)) {
                        return candidate;
                    }
                }
            }
            return _settingNameTemplate;
        }
    }
}
