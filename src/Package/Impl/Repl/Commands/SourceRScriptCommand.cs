// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class SourceRScriptCommand : PackageCommand {
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly bool _echo;

        public SourceRScriptCommand(IRInteractiveWorkflow interactiveWorkflow, IActiveWpfTextViewTracker activeTextViewTracker, bool echo)
            : base(RGuidList.RCmdSetGuid, echo ? RPackageCommandId.icmdSourceRScriptWithEcho : RPackageCommandId.icmdSourceRScript) {
            _interactiveWorkflow = interactiveWorkflow;
            _activeTextViewTracker = activeTextViewTracker;
            _echo = echo;
        }

        private ITextView GetActiveTextView() {
            return _activeTextViewTracker.GetLastActiveTextView(RContentTypeDefinition.ContentType);
        }

        private string GetFilePath() {
            ITextView textView = GetActiveTextView();
            return textView?.GetFilePath();
        }

        protected override void SetStatus() {
            Visible = _interactiveWorkflow.ActiveWindow != null;
            Enabled = Visible && !string.IsNullOrEmpty(GetFilePath());
        }

        protected override void Handle() {
            string filePath = GetFilePath();
            if (filePath != null) {
                // Save file before sourcing
                ITextView textView = GetActiveTextView();
                if (textView != null) {
                    if (RPackage.Current != null) {
                        textView.SaveFile();
                    }

                    var encoding = textView.TextBuffer.GetTextDocument()?.Encoding;

                    _interactiveWorkflow.ActiveWindow?.Container.Show(false);
                    _interactiveWorkflow.Operations.SourceFile(filePath, _echo, encoding);
                }
            }
        }
    }
}
