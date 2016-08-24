// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Extensions;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.InteractiveWorkflow.Commands {
    public sealed class SourceRScriptCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly bool _echo;

        public SourceRScriptCommand(IRInteractiveWorkflow interactiveWorkflow, IActiveWpfTextViewTracker activeTextViewTracker, bool echo) {
            _interactiveWorkflow = interactiveWorkflow;
            _activeTextViewTracker = activeTextViewTracker;
            _echo = echo;
        }

        public CommandStatus Status {
            get {
                var status = CommandStatus.Supported;
                if (_interactiveWorkflow.ActiveWindow == null) {
                    status |= CommandStatus.Invisible;
                } else if (RContentTypeDefinition.ContentType != _activeTextViewTracker.LastActiveTextView?.TextBuffer?.ContentType?.TypeName) {
                    status |= CommandStatus.Invisible;
                } else if (!string.IsNullOrEmpty(GetFilePath())) {
                    status |= CommandStatus.Enabled;
                }
                return status;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            string filePath = GetFilePath();
            if (filePath == null) {
                return CommandResult.NotSupported;
            }

            var textView = GetActiveTextView();
            var activeWindow = _interactiveWorkflow.ActiveWindow;
            if (textView == null || activeWindow == null) {
                return CommandResult.NotSupported;
            }

            _interactiveWorkflow.Shell.SaveFileIfDirty(filePath);
            activeWindow.Container.Show(focus: false, immediate: false);
            await _interactiveWorkflow.Operations.SourceFileAsync(filePath, _echo, textView.TextBuffer.GetEncoding());
            return CommandResult.Executed;
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
