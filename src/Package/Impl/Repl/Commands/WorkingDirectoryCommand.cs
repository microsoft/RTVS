// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.R.Host.Client.Extensions;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif
#if VS15
using PathHelper = Microsoft.VisualStudio.ProjectSystem.PathHelper;
#endif

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    public sealed class WorkingDirectoryCommand : Command, IDisposable {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private IRSession _session;

        public Task InitializationTask { get; }

        public WorkingDirectoryCommand(IRInteractiveWorkflow interactiveWorkflow) :
            base(new[] {
                new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdGetDirectoryList),
                new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSetWorkingDirectory)
            }, false) {

            _interactiveWorkflow = interactiveWorkflow;
            _session = interactiveWorkflow.RSession;
            _session.Connected += OnSessionConnected;
            _session.DirectoryChanged += OnCurrentDirectoryChanged;

            if (InitializationTask == null && _session.IsHostRunning) {
                InitializationTask = UpdateRUserDirectoryAsync();
            }
        }

        internal string UserDirectory { get; private set; }

        public void Dispose() {
            if (_session != null) {
                _session.Connected -= OnSessionConnected;
                _session.DirectoryChanged -= OnCurrentDirectoryChanged;
            }
            _session = null;
        }

        private void OnCurrentDirectoryChanged(object sender, EventArgs e) {
            FetchRWorkingDirectoryAsync().DoNotWait();
        }

        private void OnSessionConnected(object sender, EventArgs e) {
            FetchRWorkingDirectoryAsync().DoNotWait();
        }

        private async Task FetchRWorkingDirectoryAsync() {
            if (UserDirectory == null) {
                await UpdateRUserDirectoryAsync();
            }

            string directory = await _interactiveWorkflow.RSession.GetRWorkingDirectoryAsync();
            if (!string.IsNullOrEmpty(directory)) {
                RToolsSettings.Current.WorkingDirectory = directory;
            }
        }

        public override Microsoft.R.Components.Controller.CommandStatus Status(Guid group, int id) {
            return _interactiveWorkflow.ActiveWindow != null ?
                CommandStatus.SupportedAndEnabled :
                CommandStatus.Invisible;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            switch (id) {
                case RPackageCommandId.icmdGetDirectoryList:
                    // Return complete list
                    outputArg = GetFriendlyDirectoryNames();
                    break;

                case RPackageCommandId.icmdSetWorkingDirectory:
                    if (inputArg == null) {
                        // Return currently selected item
                        if (!string.IsNullOrEmpty(RToolsSettings.Current.WorkingDirectory)) {
                            outputArg = RToolsSettings.Current.WorkingDirectory.MakeRRelativePath(UserDirectory);
                        }
                    } else {
                        SetDirectory(inputArg as string);
                    }
                    break;
            }

            return CommandResult.Executed;
        }

        internal Task SetDirectory(string friendlyName) {
            string currentDirectory = GetFullPathName(RToolsSettings.Current.WorkingDirectory);
            string newDirectory = GetFullPathName(friendlyName);

            if (newDirectory != null && currentDirectory != newDirectory) {
                RToolsSettings.Current.WorkingDirectory = newDirectory.MakeRRelativePath(UserDirectory);
                _session.SetWorkingDirectoryAsync(newDirectory)
                    .SilenceException<RException>()
                    .SilenceException<MessageTransportException>()
                    .DoNotWait();
            }

            return Task.CompletedTask;
        }

        internal string[] GetFriendlyDirectoryNames() {
            return RToolsSettings.Current.WorkingDirectoryList
                .Select(x => UserDirectory.MakeRRelativePath(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        internal string GetFullPathName(string friendlyName) {
            string folder = friendlyName;
            if (friendlyName == null) {
                return folder;
            }

            if (!friendlyName.StartsWithIgnoreCase("~")) {
                return folder;
            }

            if (friendlyName.EqualsIgnoreCase("~")) {
                return UserDirectory;
            }

            return PathHelper.MakeRooted(PathHelper.EnsureTrailingSlash(UserDirectory), friendlyName.Substring(2));
        }

        private async Task UpdateRUserDirectoryAsync() {
            UserDirectory = await _interactiveWorkflow.RSession.GetRUserDirectoryAsync();
        }
    }
}
