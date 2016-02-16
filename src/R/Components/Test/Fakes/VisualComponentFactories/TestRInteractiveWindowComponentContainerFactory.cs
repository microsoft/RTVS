using System.ComponentModel.Composition;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.Test.UI.Fakes {
    [Export(typeof(IInteractiveWindowComponentContainerFactory))]
    internal sealed class TestRInteractiveWindowComponentContainerFactory : ContainerFactoryBase<IInteractiveWindowVisualComponent>, IInteractiveWindowComponentContainerFactory {
        private readonly IContentType _contentType;
        private IInteractiveWindowFactoryService InteractiveWindowFactory { get; }

        [ImportingConstructor]
        public TestRInteractiveWindowComponentContainerFactory(IInteractiveWindowFactoryService interactiveWindowFactory, IContentTypeRegistryService contentTypeRegistryService) {
            InteractiveWindowFactory = interactiveWindowFactory;
            _contentType = contentTypeRegistryService.GetContentType(RHistoryContentTypeDefinition.ContentType);
        }

        public IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator) {
            return GetOrCreate(instanceId, container => {
                var window = InteractiveWindowFactory.CreateWindow(evaluator);
                window.Properties[typeof(IContentType)] = _contentType;
                window.CurrentLanguageBuffer?.ChangeContentType(_contentType, null);
                window.TextView.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, false);

                return new RInteractiveWindowVisualComponent(window, container);
            }).Component;
        }
    }
}