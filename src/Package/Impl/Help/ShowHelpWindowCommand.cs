using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class ShowHelpWindowCommand : ShowToolWindowCommand<HelpWindowPane> {
        public ShowHelpWindowCommand() :
            base(RPackageCommandId.icmdShowHelpWindow) { }
    }
}
