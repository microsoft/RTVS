// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo("RTools")]
    [OrderPrecedence(200)]
    internal sealed class CopyItemPathCommand : IAsyncCommandGroupHandler {
        private IRInteractiveWorkflowProvider _interactiveWorkflowProvider;

        [ImportingConstructor]
        public CopyItemPathCommand(IRInteractiveWorkflowProvider interactiveWorkflowProvider) {
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
        }

        public Task<CommandStatusResult> GetCommandStatusAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdCopyItemPath && nodes.IsSingleNodePath()) {
                return Task.FromResult(new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported));
            }
            return Task.FromResult(CommandStatusResult.Unhandled);
        }

        public async Task<bool> TryHandleCommandAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdCopyItemPath) {
                var path = nodes.GetSingleNodePath();
                var directory = await _interactiveWorkflowProvider.GetOrCreate().RSession.GetRShortenedPathNameAsync(path);
                if (!string.IsNullOrEmpty(directory)) {
                    await VsAppShell.Current.DispatchOnMainThreadAsync(() => {
                        try {
                            Clipboard.SetData(DataFormats.UnicodeText, Invariant($"\"{directory}\""));
                        } catch (ExternalException) { }
                    });
                }
                return true;
            }
            return false;
        }
    }
}
