using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.History.Implementation {
    [Export(typeof(IMouseProcessorProvider))]
    [Name(nameof(HistoryWindowPaneMouseProcessor))]
    [Order(Before = "WordSelection")]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    [TextViewRole(RHistoryWindowVisualComponent.TextViewRole)]
    internal sealed class HistoryWindowPaneMouseProcessorProvider : IMouseProcessorProvider {
        private readonly IRHistoryProvider _historyProvider;

        [ImportingConstructor]
        public HistoryWindowPaneMouseProcessorProvider(IRHistoryProvider historyProvider) {
            _historyProvider = historyProvider;
        }

        public IMouseProcessor GetAssociatedProcessor(IWpfTextView wpfTextView) {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() => new HistoryWindowPaneMouseProcessor(_historyProvider.GetAssociatedRHistory(wpfTextView)));
        }
    }
}