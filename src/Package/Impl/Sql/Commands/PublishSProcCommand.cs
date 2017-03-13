// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using Microsoft.R.Components.Sql.Publish;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.Sql {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class PublishSProcCommand : ICommandGroupHandler {
        private readonly IApplicationShell _appShell;
        private readonly IProjectSystemServices _pss;
        private readonly IFileSystem _fs;
        private readonly IDacPackageServicesProvider _dacServicesProvider;
        private readonly ISettingsStorage _settings;

        [ImportingConstructor]
        public PublishSProcCommand(IApplicationShell appShell, IProjectSystemServices pss, IDacPackageServicesProvider dacServicesProvider, ISettingsStorage settings) :
            this(appShell, pss, new FileSystem(), dacServicesProvider, settings) {
        }

        public PublishSProcCommand(IApplicationShell appShell, IProjectSystemServices pss, IFileSystem fs, IDacPackageServicesProvider dacServicesProvider, ISettingsStorage settings) {
            _appShell = appShell;
            _pss = pss;
            _fs = fs;
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
                var sprocFiles = project.GetSProcFiles(_pss);
                if (sprocFiles.Any()) {
                    try {
                        // Make sure all files are saved and up to date on disk.
                        var dte = _appShell.GetGlobalService<DTE>(typeof(DTE));
                        dte.ExecuteCommand("File.SaveAll");

                        var publisher = new SProcPublisher(_appShell, _pss, _fs, _dacServicesProvider.GetDacPackageServices());
                        var settings = new SqlSProcPublishSettings(_settings);
                        publisher.Publish(settings, sprocFiles);
                    } catch (Exception ex) {
                        _appShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.SqlPublish_PublishError, ex.Message));
                    }
                } else {
                    _appShell.ShowErrorMessage(Resources.SqlPublishDialog_NoSProcFiles);
                }
            }
        }
    }
}
