using Microsoft.Languages.Editor.Classification;
using Microsoft.R.Support.Markdown.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.R.Support.Markdown.Classification
{
    /// <summary>
    /// Implements <see cref="IClassifier"/> and provides classification (colorization) of CoffeeScript items
    /// </summary>
    internal sealed class MdClassifier : TokenBasedClassifier<MdTokenType, MdToken>
    {
        public MdClassifier(ITextBuffer textBuffer, IClassificationTypeRegistryService classificationRegistryService) :
            base(textBuffer, new MdTokenizer(), new MdClassificationNameProvider(), classificationRegistryService)
        {
        }
    }
}
