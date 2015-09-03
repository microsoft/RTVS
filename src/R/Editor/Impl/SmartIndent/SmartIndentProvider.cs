using System.ComponentModel.Composition;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.SmartIndent
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Smart Indent")]
    internal class SmartIndentProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return SmartIndenter.Attach(textView);
        }
    }
}
