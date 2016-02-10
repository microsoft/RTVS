using Microsoft.R.Components.History;
using Microsoft.R.Components.History.Implementation;
using Microsoft.R.Components.Test.Stubs.VisualComponents;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks {
    public class RHistoryWindowVisualComponentFactoryMock : IRHistoryWindowVisualComponentFactory {
        private readonly ITextEditorFactoryService _textEditorFactory;

        public RHistoryWindowVisualComponentFactoryMock(ITextEditorFactoryService textEditorFactory) {
            _textEditorFactory = textEditorFactory;
        }

        public IRHistoryWindowVisualComponent Create(ITextBuffer historyTextBuffer, int instanceId = 0) {
            var container = new VisualComponentContainerStub<IRHistoryWindowVisualComponent>();
            var component = new RHistoryWindowVisualComponent(historyTextBuffer, _textEditorFactory, container);
            container.Component = component;
            return component;
        }
    }
}