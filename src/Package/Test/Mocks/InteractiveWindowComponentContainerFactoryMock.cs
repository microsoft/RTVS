// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Components.Test.Stubs.VisualComponents;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Shell.Mocks;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks {
    public class InteractiveWindowComponentContainerFactoryMock : IInteractiveWindowComponentContainerFactory {
        public IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator, IRInteractiveWorkflow workflow) {
            var tb = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
            var container = new VisualComponentContainerStub<RInteractiveWindowVisualComponent>();
            var component = new RInteractiveWindowVisualComponent(new InteractiveWindowMock(new WpfTextViewMock(tb), evaluator), container);
            container.Component = component;
            return component;
        }
    }
}
