using Microsoft.Languages.Core.Text;
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
            base(textBuffer, new RdTokenizer(), new RdClassificationNameProvider())
        {
            ClassificationRegistryService = classificationRegistryService;
        }

        protected override int GetAnchorPosition(int position)
        {
            // Walk back to the nearest content defining token
            // if any and re-tokenize from there

            for (int i = Tokens.Count - 1; i >= 0; i--)
            {
                if (Tokens[i].ContentTypeChange)
                {
                    return Tokens[i].Start;
                }
            }

            return base.GetAnchorPosition(position);
        }

        protected override void RemoveSensitiveTokens(int position, TextRangeCollection<RdToken> tokens)
        {
            if (Tokens.Count > 0)
            {
                Tokens.RemoveInRange(new TextRange(position, Tokens[Tokens.Count - 1].End));
            }

            base.RemoveSensitiveTokens(position, tokens);
        }
    }
}
