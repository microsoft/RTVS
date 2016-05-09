// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Markdown.Editor.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Publishing.Commands {
    internal sealed class PreviewHtmlCommand : PreviewCommand {
        public PreviewHtmlCommand(ITextView textView, IRInteractiveWorkflowProvider workflowProvider)
            : base(textView, (int)MdPackageCommandId.icmdPreviewHtml, workflowProvider) {
        }

        protected override string FileExtension {
            get { return "html"; }
        }

        protected override PublishFormat Format {
            get { return PublishFormat.Html; }
        }
    }
}
