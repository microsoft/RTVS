using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {

    [ExportCommandGroup("60481700-078B-11D1-AAF8-00A0C9055A90")]
    [AppliesTo("RTools")]
    [OrderPrecedence(100)]
    internal sealed class OpenRDataVsUiHierarchyWindowCommandGroupHandler : OpenRDataCommandGroupHandler {
        [ImportingConstructor]
        public OpenRDataVsUiHierarchyWindowCommandGroupHandler(UnconfiguredProject unconfiguredProject, IRSessionProvider sessionProvider)
            : base(unconfiguredProject, sessionProvider, (long)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_DoubleClick, (long)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_EnterKey) {}
    }

    [ExportCommandGroup("5EFC7975-14BC-11CF-9B2B-00AA00573819")]
    [AppliesTo("RTools")]
    [OrderPrecedence(100)]
    internal sealed class OpenRDataVsStd97CommandGroupHandler : OpenRDataCommandGroupHandler {
        [ImportingConstructor]
        public OpenRDataVsStd97CommandGroupHandler(UnconfiguredProject unconfiguredProject, IRSessionProvider sessionProvider)
            : base(unconfiguredProject, sessionProvider, (long)VSConstants.VSStd97CmdID.Open) {}
    }

    internal class OpenRDataCommandGroupHandler : IAsyncCommandGroupHandler {
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly IRSessionProvider _sessionProvider;
        private readonly long[] _commandIds;

        public OpenRDataCommandGroupHandler(UnconfiguredProject unconfiguredProject, IRSessionProvider sessionProvider, params long[] commandIds) {
            _unconfiguredProject = unconfiguredProject;
            _sessionProvider = sessionProvider;
            _commandIds = commandIds;
        }

        public Task<CommandStatusResult> GetCommandStatusAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus status) {
            if (_commandIds.Contains(commandId)) {
                if (nodes.Any(IsRData)) {
                    status |= CommandStatus.Supported | CommandStatus.Enabled;
                    return Task.FromResult(new CommandStatusResult(true, commandText, status));
                }
            }

            return Task.FromResult(CommandStatusResult.Unhandled);
        }

        public async Task<bool> TryHandleCommandAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            var session = _sessionProvider.Current;
            if (session == null) {
                return false;
            }

            if (!_commandIds.Contains(commandId)) {
                return false;
            }

            var rDataNode = nodes.Where(IsRData).LastOrDefault();
            if (rDataNode == null) {
                return false;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var messageResult = EditorShell.Current.ShowYesNoMessage(string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceIntoGlobalEnvironment, rDataNode.FilePath));
            if (!messageResult) {
                return true;
            }

            using (var evaluation = await session.BeginEvaluationAsync()) {
                var result = await evaluation.LoadWorkspace(rDataNode.FilePath);

                if (result.Error != null) {
                    var message = string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceFailedMessageFormat, rDataNode.FilePath, result.Error);
                    EditorShell.Current.ShowErrorMessage(message);
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