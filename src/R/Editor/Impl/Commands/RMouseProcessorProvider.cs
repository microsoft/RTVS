using System.ComponentModel.Composition;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Commands {
    [Export(typeof(IMouseProcessorProvider))]
    [Name(nameof(RMouseProcessor))]
    [Order(Before = "WordSelection")]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class RMouseProcessorProvider : IMouseProcessorProvider {
        public IMouseProcessor GetAssociatedProcessor(IWpfTextView wpfTextView) {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() => new RMouseProcessor(wpfTextView));
        }
    }
}
