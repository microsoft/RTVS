// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.InteractiveWorkflow.Commands {
    public sealed class SourceRScriptCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly bool _echo;
        private readonly IFileSystem _fs;

        public SourceRScriptCommand(IRInteractiveWorkflowVisual interactiveWorkflow, IActiveWpfTextViewTracker activeTextViewTracker, bool echo) {
            _interactiveWorkflow = interactiveWorkflow;
            _activeTextViewTracker = activeTextViewTracker;
            _echo = echo;
            _fs = _interactiveWorkflow.Shell.FileSystem();
        }

        public CommandStatus Status {
            get {
                var status = CommandStatus.Supported;
                if (_interactiveWorkflow.ActiveWindow == null) {
                    status |= CommandStatus.Invisible;
                } else if (RContentTypeDefinition.ContentType != _activeTextViewTracker.LastActiveTextView?.TextBuffer?.ContentType?.TypeName) {
                    status |= CommandStatus.Invisible;
                } else {
                    var filePath = GetFilePath();
                    if (!string.IsNullOrEmpty(filePath)) {
                        var session = _interactiveWorkflow.RSession;
                        if (session.IsRemote) {
                            if (_fs.FileExists(filePath)) {
                                status |= CommandStatus.Enabled;
                            }
                        } else {
                            status |= CommandStatus.Enabled;
                        }
                    }
                }
                return status;
            }
        }

        public async Task InvokeAsync() {
            string filePath = GetFilePath();
            if (filePath == null) {
                return;
            }

            var textView = GetActiveTextView();
            var activeWindow = _interactiveWorkflow.ActiveWindow;
            if (textView == null || activeWindow == null) {
                return;
            }

            _interactiveWorkflow.Shell.UI().SaveFileIfDirty(filePath);
            activeWindow.Container.Show(focus: false, immediate: false);

            var session = _interactiveWorkflow.RSession;
            if (session.IsRemote) {
                using (DataTransferSession dts = new DataTransferSession(_interactiveWorkflow.RSession, _fs)) {
                    // TODO: add progress indication and cancellation
                    string remotePath = await dts.CopyFileToRemoteTempAsync(filePath, true, null, CancellationToken.None);
                    await _interactiveWorkflow.Operations.SourceFileAsync(remotePath, _echo, textView.TextBuffer.GetEncoding());
                }
            } else {
                await _interactiveWorkflow.Operations.SourceFileAsync(filePath, _echo, textView.TextBuffer.GetEncoding());
            }
        }

        private ITextView GetActiveTextView() {
            return _activeTextViewTracker.GetLastActiveTextView(RContentTypeDefinition.ContentType);
        }

        private string GetFilePath() {
            ITextView textView = GetActiveTextView();
            return textView?.TextBuffer.GetFilePath();
        }
    }
}
