using Microsoft.Languages.Editor.Classification;
using Microsoft.R.Support.RD.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.R.Support.RD.Classification
{
    /// <summary>
    /// Implements <see cref="IClassifier"/> and provides classification (colorization) of CoffeeScript items
    /// </summary>
    internal sealed class RdClassifier : TokenBasedClassifier<RdTokenType, RdToken>
    {
        public RdClassifier(ITextBuffer textBuffer, IClassificationTypeRegistryService classificationRegistryService) :
            base(textBuffer, new RdTokenizer(), new RdClassificationNameProvider(), classificationRegistryService)
        {
        }
    }
}
