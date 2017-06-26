// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Publishing.Commands {
    internal sealed class PreviewWordCommand : PreviewCommand {
        public PreviewWordCommand(
            ITextView textView,
            IRInteractiveWorkflowProvider workflowProvider,
            IServiceContainer services) :
            base(textView, (int)MdPackageCommandId.icmdPreviewWord, workflowProvider, services) { }

        protected override string FileExtension => "docx";
        protected override PublishFormat Format => PublishFormat.Word;
    }
}
