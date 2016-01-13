using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    public sealed class WorkingDirectoryCommand : Command, IDisposable {
        private const int MaxDirectoryEntries = 8;
        private IRSessionProvider _sessionProvider;
        private string _userDirectory;

        public Task InitializationTask { get; private set; }

        public WorkingDirectoryCommand() :
            base(new CommandId[] {
                new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSelectWorkingDirectory),
                new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdGetDirectoryList),
                new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSetWorkingDirectory)
            }, false) {

            _sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            _sessionProvider.CurrentChanged += OnSessionProviderChanged;
            ConnectToSessionProvider();
        }

        private void ConnectToSessionProvider() {
            if (_sessionProvider.Current != null) {
                _sessionProvider.Current.Connected += OnSessionConnected;
                _sessionProvider.Current.DirectoryChanged += OnCurrentDirectoryChanged;
                if (InitializationTask == null) {
                    InitializationTask = GetRUserDirectoryAsync();
                }
            }
        }

        public void Dispose() {
            if (_sessionProvider != null) {
                _sessionProvider.CurrentChanged -= OnSessionProviderChanged;
                if (_sessionProvider.Current != null) {
                    _sessionProvider.Current.Connected -= OnSessionConnected;
                    _sessionProvider.Current.DirectoryChanged -= OnCurrentDirectoryChanged;
                }
                _sessionProvider = null;
            }
        }

        private void OnSessionProviderChanged(object sender, EventArgs e) {
            ConnectToSessionProvider();
        }

        private async void OnCurrentDirectoryChanged(object sender, EventArgs e) {
            RToolsSettings.Current.WorkingDirectory = await GetRWorkingDirectoryAsync();
        }

        private async void OnSessionConnected(object sender, EventArgs e) {
            RToolsSettings.Current.WorkingDirectory = await GetRWorkingDirectoryAsync();
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

        internal Task SetDirectory(string friendlyName) {
            string currentDirectory = RToolsSettings.Current.WorkingDirectory;
            string newDirectory = GetFullPathName(friendlyName);
            if (newDirectory != null && currentDirectory != newDirectory) {
                RToolsSettings.Current.WorkingDirectory = newDirectory;
                IRSessionProvider sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                return Task.Run(async () => {
                    await TaskUtilities.SwitchToBackgroundThread();
                    sessionProvider.Current.ScheduleEvaluation(async (e) => {
                        await e.SetWorkingDirectory(newDirectory);
                    });
                });
            }

            return Task.CompletedTask;
        }

        internal void SelectDirectory() {
            IVsUIShell uiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            IntPtr dialogOwner;
            uiShell.GetDialogOwnerHwnd(out dialogOwner);

            string currentDirectory = RToolsSettings.Current.WorkingDirectory;
            string newDirectory = Dialogs.BrowseForDirectory(dialogOwner, currentDirectory, Resources.ChooseDirectory);
            SetDirectory(newDirectory);
        }

        internal string[] GetFriendlyDirectoryNames() {
            return RToolsSettings.Current.WorkingDirectoryList
                .Select(GetFriendlyDirectoryName)
                .ToArray();
        }

        internal string GetFriendlyDirectoryName(string directory) {
            if (!string.IsNullOrEmpty(_userDirectory)) {
                if (directory.StartsWithIgnoreCase(_userDirectory)) {
                    var relativePath = PathHelper.MakeRelative(_userDirectory, directory);
                    if (relativePath.Length > 0) {
                        return "~/" + relativePath.Replace('\\', '/');
                    }
                    return "~";
                }
            }
            return directory;
        }

        internal string GetFullPathName(string friendlyName) {
            string folder = friendlyName;
            if (friendlyName == null) {
                return folder;
            }

            if (!friendlyName.StartsWithIgnoreCase("~")) {
                return folder;
            }

            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (friendlyName.EqualsIgnoreCase("~")) {
                return myDocuments;
            }

            return PathHelper.MakeRooted(PathHelper.EnsureTrailingSlash(myDocuments), friendlyName.Substring(2));
        }

        internal async Task<string> GetRWorkingDirectoryAsync() {
            try {
                using (IRSessionEvaluation eval = await _sessionProvider.Current.BeginEvaluationAsync(isMutating: false)) {
                    REvaluationResult result = await eval.EvaluateAsync("getwd()");
                    return ToWindowsPath(result.StringResult);
                }
            } catch (TaskCanceledException) { }
            return null;
        }

        internal Task GetRUserDirectoryAsync() {
            return Task.Run(async () => {
                using (IRSessionEvaluation eval = await _sessionProvider.Current.BeginEvaluationAsync(isMutating: false)) {
                    REvaluationResult result = await eval.EvaluateAsync("Sys.getenv('R_USER')");
                    _userDirectory = ToWindowsPath(result.StringResult);
                }
            });
        }

        private static string ToWindowsPath(string rPath) {
            return rPath.Replace('/', '\\');
        }
    }
}
