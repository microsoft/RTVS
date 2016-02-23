using System.ComponentModel.Composition;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Commands {
    [Export(typeof(IMouseProcessorProvider))]
    [Name(nameof(RMouseProcessor))]
    [Order(Before = "WordSelection")]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class RMouseProcessorProvider : IMouseProcessorProvider {
        public IMouseProcessor GetAssociatedProcessor(IWpfTextView wpfTextView) {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() => new RMouseProcessor(wpfTextView));
        }
    }
}
