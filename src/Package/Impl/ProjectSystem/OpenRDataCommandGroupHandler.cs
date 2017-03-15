// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    internal class OpenRDataCommandGroupHandler : IAsyncCommandGroupHandler {
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly long[] _commandIds;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;

        public OpenRDataCommandGroupHandler(UnconfiguredProject unconfiguredProject, IRInteractiveWorkflowProvider workflowProvider, params long[] commandIds) {
            _unconfiguredProject = unconfiguredProject;
            _workflowProvider = workflowProvider;
            _commandIds = commandIds;
        }

        public Task<CommandStatusResult> GetCommandStatusAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus status) {
            var session = _workflowProvider.GetOrCreate().RSession;
            if (session.IsHostRunning && _commandIds.Contains(commandId)) {
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

            MessageButtons messageResult = Vsshell.Current.ShowMessage(string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceIntoGlobalEnvironment, rDataNode.FilePath), MessageButtons.YesNo);
            if (messageResult == MessageButtons.No) {
                return true;
            }

            var session = _workflowProvider.GetOrCreate().RSession;
            try {
                await session.LoadWorkspaceAsync(rDataNode.FilePath);
            } catch (RException ex) {
                var message = string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceFailedMessageFormat,
                    rDataNode.FilePath, ex.Message);
                Vsshell.Current.ShowErrorMessage(message);
            } catch (OperationCanceledException) {
            }

            return true;
        }

        private bool IsRData(IProjectTree node) {
            var path = node.FilePath;
            return path != null && !_unconfiguredProject.IsOutsideProjectDirectory(path) && Path.GetExtension(path).Equals(".RData", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}