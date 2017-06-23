// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Common.Core.Services;

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export(typeof(IInteractiveWindowComponentContainerFactory))]
    internal class VsRInteractiveWindowComponentContainerFactory : IInteractiveWindowComponentContainerFactory {
        private readonly Lazy<IVsInteractiveWindowFactory2> _vsInteractiveWindowFactoryLazy;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public VsRInteractiveWindowComponentContainerFactory(
            Lazy<IVsInteractiveWindowFactory2> vsInteractiveWindowFactory,
            IContentTypeRegistryService contentTypeRegistryService,
            ICoreShell shell) {
            _vsInteractiveWindowFactoryLazy = vsInteractiveWindowFactory;
            _contentTypeRegistryService = contentTypeRegistryService;
            _shell = shell;
        }

        public IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator, IRSessionProvider sessionProvider) {
            _shell.MainThread().Assert();

            var vsf2 = _vsInteractiveWindowFactoryLazy.Value;
            var vsWindow2 = vsf2.Create(RGuidList.ReplInteractiveWindowProviderGuid, instanceId, string.Empty, evaluator,
                                   0, RGuidList.RCmdSetGuid, RPackageCommandId.replWindowToolBarId, null);

            var contentType = _contentTypeRegistryService.GetContentType(RContentTypeDefinition.ContentType);
            vsWindow2.SetLanguage(RGuidList.RLanguageServiceGuid, contentType);

            var toolWindow = (ToolWindowPane)vsWindow2;
            var componentContainer = new VisualComponentToolWindowAdapter<IInteractiveWindowVisualComponent>(toolWindow, _shell.Services);
            var component = new RInteractiveWindowVisualComponent(vsWindow2.InteractiveWindow, componentContainer, sessionProvider, _shell.Services);
            componentContainer.Component = component;

            RegisterFocusPreservingWindow(toolWindow);

            return component;
        }

        private void RegisterFocusPreservingWindow(ToolWindowPane toolWindow) {
            var frame = toolWindow.Frame as IVsWindowFrame;
            if (frame != null) {
                Guid persistenceSlot;
                if (frame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_GuidPersistenceSlot, out persistenceSlot) >= 0) {
                    var debugger = _shell.GetService<IVsDebugger6>(typeof(SVsShellDebugger));
                    debugger?.RegisterFocusPreservingWindow(persistenceSlot);
                }
            }
        }
    }
}