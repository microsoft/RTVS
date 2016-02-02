using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.History.Implementation {
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class HistorySelectionTextAdornmentFactory : IWpfTextViewCreationListener {
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("HistorySelectionTextAdornment")]
        [Order(Before = PredefinedAdornmentLayers.Outlining)]
        [TextViewRole(PredefinedTextViewRoles.Document)]
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