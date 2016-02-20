using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export(typeof(IInteractiveWindowComponentContainerFactory))]
    internal class VsRInteractiveWindowComponentContainerFactory : IInteractiveWindowComponentContainerFactory {
        private readonly IContentType _contentType;
        private readonly Lazy<IVsInteractiveWindowFactory> _vsInteractiveWindowFactoryLazy;

        [ImportingConstructor]
        public VsRInteractiveWindowComponentContainerFactory(Lazy<IVsInteractiveWindowFactory> vsInteractiveWindowFactory, IContentTypeRegistryService contentTypeRegistryService) {
            _vsInteractiveWindowFactoryLazy = vsInteractiveWindowFactory;
            _contentType = contentTypeRegistryService.GetContentType(RContentTypeDefinition.ContentType);
        }

        public IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator) {
            VsAppShell.Current.AssertIsOnMainThread();
            var vsWindow = _vsInteractiveWindowFactoryLazy.Value.Create(RGuidList.ReplInteractiveWindowProviderGuid, instanceId, Resources.ReplWindowName, evaluator);
            vsWindow.SetLanguage(RGuidList.RLanguageServiceGuid, _contentType);

            var toolWindow = (ToolWindowPane) vsWindow;
            ((IVsWindowFrame)toolWindow.Frame).SetProperty((int)__VSFPROPID4.VSFPROPID_TabImage, Resources.ReplWindowIcon);

            var componentContainer = new VisualComponentToolWindowAdapter<IInteractiveWindowVisualComponent>(toolWindow);
            var component = new RInteractiveWindowVisualComponent(vsWindow.InteractiveWindow, componentContainer);
            componentContainer.Component = component;
            return component;
        }
    }
}