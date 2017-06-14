// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IInteractiveWindowComponentContainerFactory))]
    internal sealed class TestRInteractiveWindowComponentContainerFactory : ContainerFactoryBase<IInteractiveWindowVisualComponent>, IInteractiveWindowComponentContainerFactory {
        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private readonly ICoreShell _shell;
        private IInteractiveWindow _window;
        private IInteractiveWindowFactoryService InteractiveWindowFactory { get; }

        [ImportingConstructor]
        public TestRInteractiveWindowComponentContainerFactory(IInteractiveWindowFactoryService interactiveWindowFactory, IContentTypeRegistryService contentTypeRegistryService, ICoreShell shell) {
            _contentTypeRegistryService = contentTypeRegistryService;
            _shell = shell;
            InteractiveWindowFactory = interactiveWindowFactory;
        }

        public IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator, IRSessionProvider sessionProvider) {
            return GetOrCreate(instanceId, container => {
                _window = InteractiveWindowFactory.CreateWindow(evaluator);
                var contentType = _contentTypeRegistryService.GetContentType(RHistoryContentTypeDefinition.ContentType);
                _window.Properties[typeof(IContentType)] = contentType;
                _window.CurrentLanguageBuffer?.ChangeContentType(contentType, null);
                _window.TextView.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, false);

                return new RInteractiveWindowVisualComponent(_window, container, sessionProvider, _shell.Services);
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