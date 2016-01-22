using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.History {
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    [TextViewRole(RHistory.TextViewRole)]
    internal sealed class HistorySelectionTextAdornmentFactory : IWpfTextViewCreationListener {
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("HistorySelectionTextAdornment")]
        [Order(Before = PredefinedAdornmentLayers.Outlining)]
        [TextViewRole(RHistory.TextViewRole)]
        public AdornmentLayerDefinition HistorySelectionTextAdornmentLayer { get; set; }

        private readonly IEditorFormatMapService _editorFormatMapService;
        private readonly IRHistoryProvider _historyProvider;

        [ImportingConstructor]
        public HistorySelectionTextAdornmentFactory(IEditorFormatMapService editorFormatMapService, IRHistoryProvider historyProvider) {
            _editorFormatMapService = editorFormatMapService;
            _historyProvider = historyProvider;
        }

        public void TextViewCreated(IWpfTextView textView) {
            textView.Properties.GetOrCreateSingletonProperty(() => new HistorySelectionTextAdornment(textView, _editorFormatMapService, _historyProvider));
        }
    }
}