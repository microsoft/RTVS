// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using EnvDTE;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Sql.Publish;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Sql {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class PublishSProcCommand : ICommandGroupHandler {
        private readonly ICoreShell _shell;
        private readonly IProjectSystemServices _pss;
        private readonly IDacPackageServicesProvider _dacServicesProvider;
        private readonly ISettingsStorage _settings;

        [ImportingConstructor]
        public PublishSProcCommand(ICoreShell shell, IProjectSystemServices pss, IDacPackageServicesProvider dacServicesProvider) :
            this(shell, pss, dacServicesProvider, shell.GetService<ISettingsStorage>()) {
        }

        public PublishSProcCommand(ICoreShell shell, IProjectSystemServices pss, IDacPackageServicesProvider dacServicesProvider, ISettingsStorage settings) {
            _shell = shell;
            _pss = pss;
            _dacServicesProvider = dacServicesProvider;
            _settings = settings;
        }

        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdPublishSProc) {
                return new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdPublishSProc) {
                if (_dacServicesProvider.GetDacPackageServices(showMessage: true) != null) {
                    Handle();
                }
                return true;
            }
            return false;
        }

        private void Handle() {
            var project = _pss.GetSelectedProject<IVsHierarchy>()?.GetDTEProject();
            if (project != null) {
                var sprocFiles = project.GetSProcFiles(_pss).ToArray();
                if (sprocFiles.Any()) {
                    try {
                        // Make sure all files are saved and up to date on disk.
                        var dte = _shell.GetService<DTE>(typeof(DTE));
                        dte.ExecuteCommand("File.SaveAll");

                        var publisher = new SProcPublisher(_shell.Services, _pss, _dacServicesProvider.GetDacPackageServices());
                        var settings = new SqlSProcPublishSettings(_settings);
                        publisher.Publish(settings, sprocFiles);
                    } catch (Exception ex) {
                        _shell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.SqlPublish_PublishError, ex.Message));
                    }
                } else {
                    _shell.ShowErrorMessage(Resources.SqlPublishDialog_NoSProcFiles);
                }
            }
        }
    }
}
