// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.Shell.Interop;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal abstract class ConfigurationSettingCommand : SessionCommand {
        private readonly string _settingNameTemplate;
        private readonly int _id;
        private readonly IProjectSystemServices _projectSystemServices;
        private readonly IProjectConfigurationSettingsProvider _projectConfigurationSettingsProvider;

        protected ConfigurationSettingCommand(
                int id, string settingNameTemplate,
                IProjectSystemServices pss,
                IProjectConfigurationSettingsProvider pcsp,
                IRInteractiveWorkflow workflow) : base(id, workflow) {
            _id = id;
            _settingNameTemplate = settingNameTemplate;
            _projectSystemServices = pss;
            _projectConfigurationSettingsProvider = pcsp;
        }

        protected async Task SaveSetting(string value) {
            string name = null;
            var hier = _projectSystemServices.GetSelectedProject<IVsHierarchy>();
            var configuredProject = hier?.GetConfiguredProject();
            if (configuredProject != null) {
                using (var access = await _projectConfigurationSettingsProvider.OpenProjectSettingsAccessAsync(configuredProject)) {
                    name = access.Settings.FindNextAvailableSettingName(_settingNameTemplate);
                    var s = new ConfigurationSetting(name, value ?? name, ConfigurationSettingValueType.String) {
                        EditorType = ConnectionStringEditor.ConnectionStringEditorName,
                        Category = ConnectionStringEditor.ConnectionStringEditorCategory,
                        Description = Resources.ConnectionStringDescription
                    };
                    access.Settings.Add(s);
                }
            }

            if(Workflow.RSession.IsHostRunning) {
                var expr = Invariant($"if (!exists('settings')) {{ settings <- as.environment(list()); }}; if (is.environment(settings)) {{ settings${name ?? _settingNameTemplate} = {value.ToRStringLiteral()}; }}");
                Workflow.Operations.EnqueueExpression(expr, addNewLine: true);
            }
        }
    }
}
