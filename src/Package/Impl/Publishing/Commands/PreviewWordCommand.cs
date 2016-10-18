// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Publishing.Commands {
    internal sealed class PreviewWordCommand : PreviewCommand {
        public PreviewWordCommand(
            ITextView textView,
            IRInteractiveWorkflowProvider workflowProvider,
            IApplicationShell appShell,
            IProcessServices pss,
            IFileSystem fs) :
            base(textView, (int)MdPackageCommandId.icmdPreviewWord, workflowProvider, appShell, pss, fs) { }

        protected override string FileExtension => "docx";
        protected override PublishFormat Format => PublishFormat.Word;
    }
}
