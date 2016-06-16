// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [Export(typeof(IInteractiveWindowComponentContainerFactory))]
    internal sealed class TestRInteractiveWindowComponentContainerFactory : ContainerFactoryBase<IInteractiveWindowVisualComponent>, IInteractiveWindowComponentContainerFactory {
        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private IInteractiveWindowFactoryService InteractiveWindowFactory { get; }

        [ImportingConstructor]
        public TestRInteractiveWindowComponentContainerFactory(IInteractiveWindowFactoryService interactiveWindowFactory, IContentTypeRegistryService contentTypeRegistryService) {
            _contentTypeRegistryService = contentTypeRegistryService;
            InteractiveWindowFactory = interactiveWindowFactory;
        }

        public IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator) {
            return GetOrCreate(instanceId, container => {
                var window = InteractiveWindowFactory.CreateWindow(evaluator);
                var contentType = _contentTypeRegistryService.GetContentType(RHistoryContentTypeDefinition.ContentType);
                window.Properties[typeof(IContentType)] = contentType;
                window.CurrentLanguageBuffer?.ChangeContentType(contentType, null);
                window.TextView.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, false);

                return new RInteractiveWindowVisualComponent(window, container);
            }).Component;
        }
    }
}