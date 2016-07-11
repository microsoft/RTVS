// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using static System.FormattableString;
using Microsoft.R.Host.Client;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
#endif


namespace Microsoft.VisualStudio.R.Package.Sql {
    internal abstract class ConfigurationSettingCommand : IAsyncCommandGroupHandler {
        private readonly string _settingNameTemplate;
        private readonly int _id;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;
        private readonly ConfiguredProject _configuredProject;
        private readonly IProjectConfigurationSettingsProvider _projectConfigurationSettingsProvider;

        public ConfigurationSettingCommand(int id) : this(id, null, null, null, null) { }

        public ConfigurationSettingCommand(
                int id,
                string settingNameTemplate,
                ConfiguredProject configuredProject,
                IProjectConfigurationSettingsProvider pcsp,
                IRInteractiveWorkflowProvider workflowProvider) {
            _id = id;
            _settingNameTemplate = settingNameTemplate;
            _workflowProvider = workflowProvider;
            _configuredProject = configuredProject;
            _projectConfigurationSettingsProvider = pcsp;
        }

        public Task<CommandStatusResult> GetCommandStatusAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == _id) {
                return Task.FromResult(new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported));
            }
            return Task.FromResult(CommandStatusResult.Unhandled);
        }

        public Task<bool> TryHandleCommandAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == _id) {
                return TryHandleCommandAsync();
            }
            return Task.FromResult(false);
        }

        public abstract Task<bool> TryHandleCommandAsync();

        protected async Task SaveSetting(string value) {
            using (var access = await _projectConfigurationSettingsProvider.OpenProjectSettingsAccessAsync(_configuredProject)) {
                var name = access.Settings.FindNextAvailableSettingName(_settingNameTemplate);
                var s = new ConfigurationSetting(name, value ?? name, ConfigurationSettingValueType.String);
                s.EditorType = ConnectionStringEditor.ConnectionStringEditorName;
                s.Category = ConnectionStringEditor.ConnectionStringEditorCategory;
                s.Description = Resources.ConnectionStringDescription;
                access.Settings.Add(s);

                var session = _workflowProvider.GetOrCreate()?.RSession;
                await session?.EvaluateAsync(Invariant($"{name} <- '{value}'"), REvaluationKind.Mutating);
            }
        }
    }
}
