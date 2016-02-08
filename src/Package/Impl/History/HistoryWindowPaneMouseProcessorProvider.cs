using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.History {
    [Export(typeof(IMouseProcessorProvider))]
    [Name("HistoryWindowPaneMouseProcessor")]
    [Order(Before = "WordSelection")]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    [TextViewRole(RHistory.TextViewRole)]
    internal sealed class HistoryWindowPaneMouseProcessorProvider : IMouseProcessorProvider {
        private readonly IRHistoryProvider _historyProvider;

        [ImportingConstructor]
        public HistoryWindowPaneMouseProcessorProvider(IRHistoryProvider historyProvider) {
            _historyProvider = historyProvider;
        }

        public IMouseProcessor GetAssociatedProcessor(IWpfTextView wpfTextView) {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() => new HistoryWindowPaneMouseProcessor(wpfTextView, _historyProvider, EditorShell.Current));
        }
    }
}