// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.ProjectSystem;
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

            var workflow = _workflowProvider.GetOrCreate();
            var services = workflow.Shell.Services;
            var messageResult = services.ShowMessage(Resources.LoadWorkspaceIntoGlobalEnvironment.FormatCurrent(rDataNode.FilePath), MessageButtons.YesNo);
            if (messageResult == MessageButtons.No) {
                return true;
            }

            var session = workflow.RSession;
            try {
                await session.LoadWorkspaceAsync(rDataNode.FilePath);
            } catch (RException ex) {
                var message = Resources.LoadWorkspaceFailedMessageFormat.FormatCurrent(rDataNode.FilePath, ex.Message);
                services.ShowErrorMessage(message);
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