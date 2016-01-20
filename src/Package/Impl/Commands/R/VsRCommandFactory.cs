using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Controller;
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
        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var exportProvider = VsAppShell.Current.ExportProvider;
            var interactiveSessionProvider = exportProvider.GetExportedValue<IRInteractiveSessionProvider>();
            var interactiveSession = interactiveSessionProvider.GetOrCreate();

            return new ICommand[] {
                new ShowContextMenuCommand(textView, RGuidList.RPackageGuid, RGuidList.RCmdSetGuid, (int) RContextMenuId.R),
                new SendToReplCommand(textView, interactiveSession),
                new SourceRScriptCommand(textView, interactiveSession),
                new GoToFormattingOptionsCommand(textView, textBuffer),
                new WorkingDirectoryCommand()
            };
        }
    }
}
