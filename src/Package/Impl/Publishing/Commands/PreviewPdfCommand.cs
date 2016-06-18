// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Publishing.Commands {
    internal sealed class PreviewPdfCommand : PreviewCommand {
        public PreviewPdfCommand(
            ITextView textView,
            IRInteractiveWorkflowProvider workflowProvider,
            ICoreShell coreShell,
            IProcessServices pss,
            IFileSystem fs)
            : base(textView, (int)MdPackageCommandId.icmdPreviewPdf, workflowProvider, coreShell, pss, fs) { }

        protected override string FileExtension => "pdf";
         protected override PublishFormat Format => PublishFormat.Pdf;

        protected override bool CheckPrerequisites() {
            if (!base.CheckPrerequisites()) {
                return false;
            }
            if (!IOExtensions.ExistsOnPath("pdflatex.exe")) {
                VsAppShell.Current.ShowErrorMessage(Resources.Error_PdfLatexMissing);
                Process.Start("http://miktex.org/2.9/setup");
                return false;
            }
            return true;
        }
    }
}
