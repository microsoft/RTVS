// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Settings;
using Microsoft.Languages.Editor.Composition;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#else
using Microsoft.VisualStudio.ProjectSystem;
#endif

namespace Microsoft.VisualStudio.R.Package.Sql {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class PublishSProcCommand : ICommandGroupHandler {
        private readonly ICoreShell _coreShell;
        private readonly IProjectSystemServices _pss;
        private readonly IWritableSettingsStorage _settingsStorage;

        [ImportingConstructor]
        public PublishSProcCommand(ICoreShell coreShell, IProjectSystemServices pss, [Import(AllowDefault = true)]IWritableSettingsStorage settingsStorage) {
            if (settingsStorage == null) {
                var ctrs = coreShell.ExportProvider.GetExportedValue<IContentTypeRegistryService>();
                var contentType = ctrs.GetContentType(RContentTypeDefinition.ContentType);
                settingsStorage = ComponentLocatorForOrderedContentType<IWritableSettingsStorage>
                                        .FindFirstOrderedComponent(coreShell.CompositionService, contentType);
            }

            _coreShell = coreShell;
            _pss = pss;
            _settingsStorage = settingsStorage;
        }

        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdPublishSProc) {
                return new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdPublishSProc) {
                Handle();
                return true;
            }
            return false;
        }

        private void Handle() {
            var project = _pss.GetSelectedProject<IVsHierarchy>()?.GetDTEProject();
            if (project != null) {
                if (SqlPublishDialogViewModel.GetDatabaseProjectsInSolution(_pss).Count > 0) {
                    var rFiles = _pss.GetProjectFiles(project).Where(x => Path.GetExtension(x).EqualsIgnoreCase(".R"));
                    var sqlFiles = new HashSet<string>(_pss.GetProjectFiles(project).Where(x => Path.GetExtension(x).EqualsIgnoreCase(".sql")));
                    var sprocFiles = rFiles.Where(x =>
                                sqlFiles.Contains(x.ToQueryFilePath(), StringComparer.OrdinalIgnoreCase) &&
                                sqlFiles.Contains(x.ToSProcFilePath(), StringComparer.OrdinalIgnoreCase));
                    if (sprocFiles.Any()) {
                        var dlg = new SqlPublshDialog(_coreShell, _pss, _settingsStorage, sprocFiles);
                        dlg.ShowModal();
                    } else {
                        _coreShell.ShowErrorMessage(Resources.SqlPublishDialog_NoSProcFiles);
                    }
                } else {
                    _coreShell.ShowErrorMessage(Resources.SqlPublishDialog_NoDbProject);
                }
            }
        }
    }
}
