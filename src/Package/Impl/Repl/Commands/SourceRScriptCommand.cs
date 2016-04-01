// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        public SourceRScriptCommand(IRInteractiveWorkflow interactiveWorkflow, IActiveWpfTextViewTracker activeTextViewTracker)
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSourceRScript) {
            _interactiveWorkflow = interactiveWorkflow;
            _activeTextViewTracker = activeTextViewTracker;
        }

        private ITextView GetActiveTextView() {
            return _activeTextViewTracker.GetLastActiveTextView(RContentTypeDefinition.ContentType);
        }

        private string GetFilePath() {
            ITextView textView = GetActiveTextView();
            return textView.GetFilePath();
        }

        protected override void SetStatus() {
            Visible = _interactiveWorkflow.ActiveWindow != null && _interactiveWorkflow.ActiveWindow.Container.IsOnScreen;
            Enabled = GetFilePath() != null;
        }

        protected override void Handle() {
            string filePath = GetFilePath();
            if (filePath != null) {
                // Save file before sourcing
                ITextView textView = GetActiveTextView();
                textView.SaveFile();
                _interactiveWorkflow.Operations.SourceFile(filePath);
            }
        }
    }
}
