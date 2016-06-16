// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Extensions;
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
        private readonly Lazy<IVsInteractiveWindowFactory> _vsInteractiveWindowFactoryLazy;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;

        [ImportingConstructor]
        public VsRInteractiveWindowComponentContainerFactory(Lazy<IVsInteractiveWindowFactory> vsInteractiveWindowFactory, IContentTypeRegistryService contentTypeRegistryService) {
            _vsInteractiveWindowFactoryLazy = vsInteractiveWindowFactory;
            _contentTypeRegistryService = contentTypeRegistryService;
        }

        public IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator) {
            VsAppShell.Current.AssertIsOnMainThread();

            var vsWindow = _vsInteractiveWindowFactoryLazy.Value.Create(RGuidList.ReplInteractiveWindowProviderGuid, instanceId, Resources.ReplWindowName, evaluator);
            var contentType = _contentTypeRegistryService.GetContentType(RContentTypeDefinition.ContentType);
            vsWindow.SetLanguage(RGuidList.RLanguageServiceGuid, contentType);

            var toolWindow = (ToolWindowPane) vsWindow;
            var frame = (IVsWindowFrame)toolWindow.Frame;
            frame.SetProperty((int)__VSFPROPID4.VSFPROPID_TabImage, Resources.ReplWindowIcon);
            // TODO: figure out why REPL window doesn't get 'force create' flag set
            // For now, set it forcibly when window is shown
            object value;
            frame.GetProperty((int)__VSFPROPID.VSFPROPID_CreateToolWinFlags, out value);
            frame.SetProperty((int)__VSFPROPID.VSFPROPID_CreateToolWinFlags, (int)value | (int)__VSCREATETOOLWIN.CTW_fForceCreate);

            var componentContainer = new VisualComponentToolWindowAdapter<IInteractiveWindowVisualComponent>(toolWindow);
            var component = new RInteractiveWindowVisualComponent(vsWindow.InteractiveWindow, componentContainer);
            componentContainer.Component = component;
            return component;
        }
    }
}