using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class ShowRInteractiveWindowsCommand : PackageCommand {
        private readonly IRInteractiveWorkflowProvider _interactiveWorkflowProvider;

        public ShowRInteractiveWindowsCommand(IRInteractiveWorkflowProvider interactiveWorkflowProvider) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowReplWindow) {
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
        }

        internal override void Handle() {
            var interactiveWorkflow = _interactiveWorkflowProvider.GetOrCreate();
            var window = interactiveWorkflow.ActiveWindow;
            if (window != null) {
                window.Container.Show(true);
                return;
            }

            _interactiveWorkflowProvider
                .CreateInteractiveWindowAsync(interactiveWorkflow)
                .ContinueOnRanToCompletion(w => w.Container.Show(true));
        }
    }
}
