// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using PathHelper = Microsoft.VisualStudio.ProjectSystem.PathHelper;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    public sealed class WorkingDirectoryCommand : Command, IDisposable {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRSettings _settings;
        private IRSession _session;

        public Task InitializationTask { get; }

        public WorkingDirectoryCommand(IRInteractiveWorkflowVisual interactiveWorkflow) :
            base(new[] {
                new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdGetDirectoryList),
                new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSetWorkingDirectory)
            }, false) {

            _interactiveWorkflow = interactiveWorkflow;
            _settings = _interactiveWorkflow.Shell.GetService<IRSettings>();

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
                _settings.WorkingDirectory = directory;
            }
        }

        public override CommandStatus Status(Guid group, int id) {
            if (_interactiveWorkflow.ActiveWindow == null) {
                return CommandStatus.Invisible;
            }

            return _interactiveWorkflow.RSession.IsHostRunning && !_interactiveWorkflow.RSession.IsRemote ?
                CommandStatus.SupportedAndEnabled :
                CommandStatus.Supported;
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
                        if (!string.IsNullOrEmpty(_settings.WorkingDirectory)) {
                            outputArg = GetFriendlyDirectoryName(_settings.WorkingDirectory);
                        }
                    } else {
                        SetDirectory(inputArg as string);
                    }
                    break;
            }

            return CommandResult.Executed;
        }

        internal Task SetDirectory(string friendlyName) {
            string currentDirectory = GetFullPathName(_settings.WorkingDirectory);
            string newDirectory = GetFullPathName(friendlyName);

            if (newDirectory != null && currentDirectory != newDirectory) {
                _settings.WorkingDirectory = GetFriendlyDirectoryName(newDirectory);
                _session.SetWorkingDirectoryAsync(newDirectory)
                    .SilenceException<RException>()
                    .DoNotWait();
            }

            return Task.CompletedTask;
        }

        internal string[] GetFriendlyDirectoryNames() {
            return _settings.WorkingDirectoryList
                .Select(GetFriendlyDirectoryName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        internal string GetFriendlyDirectoryName(string directory) {
            if (!string.IsNullOrEmpty(UserDirectory)) {
                if (directory.EndsWithOrdinal("\\") && !directory.EndsWithOrdinal(":\\")) {
                    directory = directory.Substring(0, directory.Length - 1);
                }
                if (directory.StartsWithIgnoreCase(UserDirectory)) {
                    var relativePath = PathHelper.MakeRelative(UserDirectory, directory);
                    if (relativePath.Length > 0) {
                        return "~/" + relativePath.Replace('\\', '/');
                    }
                    return "~";
                }
                return directory.Replace('\\', '/');
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
