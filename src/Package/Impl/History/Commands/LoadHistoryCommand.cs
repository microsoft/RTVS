using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class LoadHistoryCommand : ViewCommand {
        private readonly IRHistory _history;

        public LoadHistoryCommand(ITextView textView, IRHistoryProvider historyProvider)
            : base(textView, RGuidList.RCmdSetGuid, RPackageCommandId.icmdLoadHistory, false) {
            _history = historyProvider.GetAssociatedRHistory(textView);
        }

        public override CommandStatus Status(Guid guid, int id) {
            return ReplWindow.ReplWindowExists() ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var initialPath = RToolsSettings.Current.WorkingDirectory != null ? PathHelper.EnsureTrailingSlash(RToolsSettings.Current.WorkingDirectory) : null;
            var file = VsAppShell.Current.BrowseForFileOpen(IntPtr.Zero, Resources.HistoryFileFilter, initialPath, Resources.LoadHistoryTitle);
            if (file != null) {
                _history.TryLoadFromFile(file);
            }

            return CommandResult.Executed;
        }
    }
}
