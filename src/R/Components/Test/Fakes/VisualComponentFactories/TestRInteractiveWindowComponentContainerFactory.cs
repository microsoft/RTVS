// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [Export(typeof(IInteractiveWindowComponentContainerFactory))]
    internal sealed class TestRInteractiveWindowComponentContainerFactory : ContainerFactoryBase<IInteractiveWindowVisualComponent>, IInteractiveWindowComponentContainerFactory {
        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private IInteractiveWindow _window;
        private IInteractiveWindowFactoryService InteractiveWindowFactory { get; }

        [ImportingConstructor]
        public TestRInteractiveWindowComponentContainerFactory(IInteractiveWindowFactoryService interactiveWindowFactory, IContentTypeRegistryService contentTypeRegistryService) {
            _contentTypeRegistryService = contentTypeRegistryService;
            InteractiveWindowFactory = interactiveWindowFactory;
        }

        public IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator, IRInteractiveWorkflow workflow) {
            return GetOrCreate(instanceId, container => {
                _window = InteractiveWindowFactory.CreateWindow(evaluator);
                var contentType = _contentTypeRegistryService.GetContentType(RHistoryContentTypeDefinition.ContentType);
                _window.Properties[typeof(IContentType)] = contentType;
                _window.CurrentLanguageBuffer?.ChangeContentType(contentType, null);
                _window.TextView.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, false);

                return new RInteractiveWindowVisualComponent(_window, container);
            }).Component;
        }

        public override void Dispose() {
            UIThreadHelper.Instance.Invoke(() => {
                _window?.TextView?.Close();
                _window?.Dispose();
            });
            base.Dispose();
        }
    }
}