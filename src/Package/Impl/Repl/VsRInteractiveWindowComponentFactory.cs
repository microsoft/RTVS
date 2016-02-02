using System.ComponentModel.Composition;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export(typeof(IInteractiveWindowComponentFactory))]
    internal class VsRInteractiveWindowComponentFactory : IInteractiveWindowComponentFactory {
        private IVsInteractiveWindowFactory VsInteractiveWindowFactory { get; }

        [ImportingConstructor]
        public VsRInteractiveWindowComponentFactory(IVsInteractiveWindowFactory vsInteractiveWindowFactory) {
            VsInteractiveWindowFactory = vsInteractiveWindowFactory;
        }

        public IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator) {
            var vsWindow = VsInteractiveWindowFactory.Create(RGuidList.ReplInteractiveWindowProviderGuid, instanceId, Resources.ReplWindowName, evaluator);
            var toolWindow = (ToolWindowPane) vsWindow;
            ((IVsWindowFrame)toolWindow.Frame).SetProperty((int)__VSFPROPID4.VSFPROPID_TabImage, Resources.ReplWindowIcon);

            var componentContainer = new VisualComponentToolWindowAdapter<IInteractiveWindowVisualComponent>(toolWindow);
            var component = new RInteractiveWindowVisualComponent(vsWindow.InteractiveWindow, componentContainer);
            componentContainer.Component = component;
            return component;
        }
    }
}