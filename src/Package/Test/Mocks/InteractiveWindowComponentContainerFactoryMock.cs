// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Components.Test.Stubs.VisualComponents;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Shell.Mocks;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks {
    public class InteractiveWindowComponentContainerFactoryMock : IInteractiveWindowComponentContainerFactory {
        private readonly ICoreShell _shell;

        public InteractiveWindowComponentContainerFactoryMock(ICoreShell shell) {
            _shell = shell;
        }

        public IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator, IRSessionProvider sessionProvider) {
            var tb = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
            var container = new VisualComponentContainerStub<RInteractiveWindowVisualComponent>();
            var component = new RInteractiveWindowVisualComponent(new InteractiveWindowMock(new WpfTextViewMock(tb), evaluator), container, sessionProvider, _shell.Services);
            container.Component = component;
            return component;
        }
    }
}
