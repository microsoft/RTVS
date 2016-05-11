// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    internal class OpenRDataCommandGroupHandler : IAsyncCommandGroupHandler {
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly long[] _commandIds;
        private readonly IRSession _session;

        public OpenRDataCommandGroupHandler(UnconfiguredProject unconfiguredProject, IRSessionProvider sessionProvider, params long[] commandIds) {
            _unconfiguredProject = unconfiguredProject;
            _session = sessionProvider.GetInteractiveWindowRSession();
            _commandIds = commandIds;
        }

        public Task<CommandStatusResult> GetCommandStatusAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus status) {
            if (_session.IsHostRunning && _commandIds.Contains(commandId)) {
                if (nodes.Any(IsRData)) {
                    status |= CommandStatus.Supported | CommandStatus.Enabled;
                    return Task.FromResult(new CommandStatusResult(true, commandText, status));
                }
            }

            return Task.FromResult(CommandStatusResult.Unhandled);
        }

        public async Task<bool> TryHandleCommandAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (!_commandIds.Contains(commandId)) {
                return false;
            }

            var rDataNode = nodes.Where(IsRData).LastOrDefault();
            if (rDataNode == null) {
                return false;
            }

            return await TryHandleCommandAsyncInternal(rDataNode);
        }

        protected virtual async Task<bool> TryHandleCommandAsyncInternal(IProjectTree rDataNode) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            MessageButtons messageResult = VsAppShell.Current.ShowMessage(string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceIntoGlobalEnvironment, rDataNode.FilePath), MessageButtons.YesNo);
            if (messageResult == MessageButtons.No) {
                return true;
            }

            using (var evaluation = await _session.BeginEvaluationAsync()) {
                try {
                    await evaluation.LoadWorkspace(rDataNode.FilePath);
                } catch (RException ex) {
                    var message = string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceFailedMessageFormat,
                        rDataNode.FilePath, ex.Message);
                    VsAppShell.Current.ShowErrorMessage(message);
                } catch (OperationCanceledException) {
                }
            }

            return true;
        }

        private bool IsRData(IProjectTree node) {
            var path = node.FilePath;
            return path != null && !_unconfiguredProject.IsOutsideProjectDirectory(path) && Path.GetExtension(path).Equals(".RData", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}