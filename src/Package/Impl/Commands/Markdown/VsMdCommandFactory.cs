using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands.Markdown;
using Microsoft.VisualStudio.R.Package.Publishing.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.Markdown;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.MD {
    [Export(typeof(ICommandFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal class VsMdCommandFactory : ICommandFactory {
        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var exportProvider = VsAppShell.Current.ExportProvider;
            var interactiveWorkflowProvider = exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            var interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();

            if (interactiveWorkflow.ActiveWindow == null) {
                interactiveWorkflowProvider
                    .CreateInteractiveWindowAsync(interactiveWorkflow)
                    .ContinueOnRanToCompletion(w => w.Container.Show(false));
            }

            return new ICommand[] {
                new PreviewHtmlCommand(textView),
                new PreviewPdfCommand(textView),
                new PreviewWordCommand(textView),
                new SendToReplCommand(textView, interactiveWorkflow),
                new ShowContextMenuCommand(textView, MdGuidList.MdPackageGuid, MdGuidList.MdCmdSetGuid, (int) MarkdownContextMenuId.MD)
            };
        }
    }
}
