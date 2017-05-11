// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Documentation.Commands {
    public sealed class OpenDocumentationCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly string _onlineUrl;
        private readonly string _localRelativePath;

        public OpenDocumentationCommand(IRInteractiveWorkflow interactiveWorkflow, string onlineUrl,
            string localRelativePath = null) {
            _interactiveWorkflow = interactiveWorkflow;
            _onlineUrl = onlineUrl;
            _localRelativePath = localRelativePath;
        }

        public CommandStatus Status => CommandStatus.Supported | CommandStatus.Enabled;

        public Task InvokeAsync() {
            var url = _onlineUrl;

            if (!string.IsNullOrEmpty(_localRelativePath)) {
                var active = _interactiveWorkflow.Connections.ActiveConnection;
                if (active?.IsRemote == false) {
                    var localPath = Path.Combine(active.Path, _localRelativePath);
                    if (File.Exists(localPath)) {
                        url = localPath;
                    }
                }
            }

            OpenUrl(url);

            return Task.CompletedTask;
        }

        private void OpenUrl(string url) {
            ProcessStartInfo psi = new ProcessStartInfo {
                UseShellExecute = true,
                FileName = url
            };
            Process.Start(psi);
        }
    }
}