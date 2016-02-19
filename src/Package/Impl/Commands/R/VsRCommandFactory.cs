using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.R {
    [Export(typeof(ICommandFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal class VsRCommandFactory : ICommandFactory {
        private readonly IRInteractiveWorkflowProvider _workflowProvider;

        [ImportingConstructor]
        public VsRCommandFactory(IRInteractiveWorkflowProvider workflowProvider) {
            _workflowProvider = workflowProvider;
        }

        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var workflow = _workflowProvider.GetOrCreate();

            if (workflow.ActiveWindow == null) {
                _workflowProvider
                    .CreateInteractiveWindowAsync(workflow)
                    .ContinueOnRanToCompletion(w => w.Container.Show(false));
            }

            return new ICommand[] {
                new ShowContextMenuCommand(textView, RGuidList.RPackageGuid, RGuidList.RCmdSetGuid, (int) RContextMenuId.R),
                new SendToReplCommand(textView, workflow),
                new GoToFormattingOptionsCommand(textView, textBuffer),
                new WorkingDirectoryCommand(workflow)
            };
        }
    }
}
