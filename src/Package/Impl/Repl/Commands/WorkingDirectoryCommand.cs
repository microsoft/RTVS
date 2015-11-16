using System;
using System.Collections.Generic;
using System.IO;
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
    public sealed class WorkingDirectoryCommand : ViewCommand {
        private const int MaxDirectoryEntries = 8;

        public WorkingDirectoryCommand(ITextView textView) :
            base(textView, new CommandId[] {
                new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSelectWorkingDirectory),
                new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdGetDirectoryList),
                new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSetWorkingDirectory)
            }, false) {
        }

        public override CommandStatus Status(Guid group, int id) {
            if (ReplWindow.ReplWindowExists()) {
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            switch (id) {
                case (int)RPackageCommandId.icmdSelectWorkingDirectory:
                    SelectDirectory();
                    break;

                case (int)RPackageCommandId.icmdGetDirectoryList:
                    // Return complete list
                    outputArg = GetFriendlyDirectoryNames();
                    break;

                case (int)RPackageCommandId.icmdSetWorkingDirectory:
                    if (inputArg == null) {
                        // Return currently selected item
                        if (!string.IsNullOrEmpty(RToolsSettings.Current.WorkingDirectory)) {
                            outputArg = GetFriendlyDirectoryName(RToolsSettings.Current.WorkingDirectory);
                        }
                    } else {
                        SetDirectory(inputArg as string);
                    }
                    break;
            }

            return CommandResult.Executed;
        }

        private Task SetDirectory(string friendlyName) {
            string currentDirectory = RToolsSettings.Current.WorkingDirectory;
            string newDirectory = GetFullPathName(friendlyName);
            if (newDirectory != null && currentDirectory != newDirectory) {
                RToolsSettings.Current.WorkingDirectory = newDirectory;
                IRSessionProvider sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                var sessions = sessionProvider.GetSessions();
                return Task.Run(async () => {
                    await TaskUtilities.SwitchToBackgroundThread();
                    foreach (var s in sessions.Values) {
                        s.ScheduleEvaluation(async (e) => {
                            await e.SetWorkingDirectory(newDirectory);
                        });
                    }
                });
            }

            return Task.CompletedTask;
        }

        private void SelectDirectory() {
            IVsUIShell uiShell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            IntPtr dialogOwner;
            uiShell.GetDialogOwnerHwnd(out dialogOwner);

            string currentDirectory = RToolsSettings.Current.WorkingDirectory;
            string newDirectory = Dialogs.BrowseForDirectory(dialogOwner, currentDirectory, Resources.ChooseDirectory);
            SetDirectory(newDirectory);
        }

        private string[] GetFriendlyDirectoryNames() {
            List<string> friendlyNames = new List<string>();
            foreach (var dir in RToolsSettings.Current.WorkingDirectoryList) {
                friendlyNames.Add(GetFriendlyDirectoryName(dir));
            }
            return friendlyNames.ToArray();
        }

        private string GetFriendlyDirectoryName(string directory) {
            string userName = Environment.UserName;

            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (directory.StartsWithIgnoreCase(myDocuments)) {
                return ExtractFriendlyName(directory);
            }

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (directory.StartsWithIgnoreCase(desktop)) {
                return ExtractFriendlyName(directory);
            }

            return directory;
        }

        private string ExtractFriendlyName(string folder) {
            string userName = Environment.UserName;
            int index = folder.IndexOf("\\" + userName + "\\");
            if (index >= 0) {
                return "~/" + folder.Substring(index + userName.Length + 2).Replace('\\', '/');
            }
            return folder;
        }

        private string GetFullPathName(string friendlyName) {
            string folder = friendlyName;
            if (friendlyName != null) {
                if (friendlyName.StartsWithIgnoreCase("~/")) {
                    string userName = Environment.UserName;
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    int index = desktopPath.IndexOf("\\" + userName + "\\", StringComparison.OrdinalIgnoreCase);
                    if (index >= 0) {
                        folder = Path.Combine(desktopPath.Substring(0, index + userName.Length + 2), friendlyName.Substring(2).Replace('/', '\\'));
                    }
                }
            }
            return folder;
        }
    }
}
