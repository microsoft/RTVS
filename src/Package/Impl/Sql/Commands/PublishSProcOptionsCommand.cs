// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Components.Sql.Publish;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Sql.Publish;

namespace Microsoft.VisualStudio.R.Package.Sql {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class PublishSProcOptionsCommand : IAsyncCommandGroupHandler {
        private readonly ICoreShell _shell;
        private readonly IProjectSystemServices _pss;
        private readonly IProjectConfigurationSettingsProvider _pcsp;
        private readonly IDacPackageServicesProvider _dacServicesProvider;
        private readonly ISettingsStorage _settings;

        [ImportingConstructor]
        public PublishSProcOptionsCommand(ICoreShell shell, IProjectSystemServices pss, 
                                          IProjectConfigurationSettingsProvider pcsp, IDacPackageServicesProvider dacServicesProvider) {
            _shell = shell;
            _pss = pss;
            _pcsp = pcsp;
            _dacServicesProvider = dacServicesProvider;
            _settings = shell.GetService<ISettingsStorage>();
        }

        public Task<CommandStatusResult> GetCommandStatusAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdPublishSProcOptions) {
                return Task.FromResult(new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported));
            }
            return Task.FromResult(CommandStatusResult.Unhandled);
        }

        public async Task<bool> TryHandleCommandAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdPublishSProcOptions) {
                if (_dacServicesProvider.GetDacPackageServices() != null) {
                    var dlg = await SqlPublshOptionsDialog.CreateAsync(_shell, _pss, _pcsp, _settings);
                    await dlg.InitializeAsync();
                    await _shell.MainThread().SwitchToAsync();
                    dlg.ShowModal();
                } 
                return true;
            }
            return false;
        }
    }
}
