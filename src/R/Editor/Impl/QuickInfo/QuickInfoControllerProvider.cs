using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.QuickInfo
{
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("R ToolTip QuickInfo Controller")]
    [ContentType(RContentTypeDefinition.ContentType)]
    sealed class QuickInfoControllerProvider : IIntellisenseControllerProvider
    {
        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            return new QuickInfoController(textView, subjectBuffers);
        }
    }
}
