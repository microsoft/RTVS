using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Test.Stubs.VisualComponents;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.Shell.Mocks;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks {
    public class InteractiveWindowComponentFactoryMock : IInteractiveWindowComponentFactory {
        public IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator) {
            var tb = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
            var container = new VisualComponentContainerStub<RInteractiveWindowVisualComponent>();
            var component = new RInteractiveWindowVisualComponent(new InteractiveWindowMock(new WpfTextViewMock(tb)), container);
            container.Component = component;
            return component;
        }
    }
}
