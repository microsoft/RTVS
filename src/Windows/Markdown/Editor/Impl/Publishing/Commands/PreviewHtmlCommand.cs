// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Text.Editor;
using static System.FormattableString;

namespace Microsoft.Markdown.Editor.Publishing.Commands {
    internal sealed class PreviewHtmlCommand : PreviewCommand {

        public PreviewHtmlCommand(
            ITextView textView,
            IRInteractiveWorkflowProvider workflowProvider, IServiceContainer services)
            : base(textView, MdPackageCommandId.icmdPreviewHtml, workflowProvider, services) {
        }

        protected override string FileExtension=> "html";
        protected override PublishFormat Format=> PublishFormat.Html;

        protected override void LaunchViewer(string fileName) {
            Services.Process().Start(Invariant($"file://{fileName}"));
        }
    }
}
