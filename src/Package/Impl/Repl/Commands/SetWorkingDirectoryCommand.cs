using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudioTools;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class SetWorkingDirectoryCommand : ViewCommand {
        public SetWorkingDirectoryCommand(ITextView textView) :
            base(textView, new CommandId(RGuidList.RCmdSetGuid, (int)RPackageCommandId.icmdSetWorkingDirectory), false) {
        }

        public override CommandStatus Status(Guid group, int id) {
            if (ReplWindow.ReplWindowExists()) {
                return CommandStatus.SupportedAndEnabled;
            }

            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            IVsUIShell uiShell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            IntPtr dialogOwner;
            uiShell.GetDialogOwnerHwnd(out dialogOwner);

            string currentDirectory = RToolsSettings.Current.WorkingDirectory;
            string newDirectory = Dialogs.BrowseForDirectory(dialogOwner, currentDirectory, Resources.ChooseDirectory);
            if (newDirectory != null && currentDirectory != newDirectory) {
                IRSessionProvider sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                var sessions = sessionProvider.GetSessions();
                Task.Run(async () => {
                    await TaskUtilities.SwitchToBackgroundThread();
                    foreach (var s in sessions.Values) {
                        s.ScheduleEvaluation(async (e) => {
                            await e.SetWorkingDirectory(newDirectory);
                        });
                    }
                });
            }
            return CommandResult.Executed;
        }
    }
}
