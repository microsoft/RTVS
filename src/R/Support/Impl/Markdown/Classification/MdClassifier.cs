using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Classification;
using Microsoft.Languages.Editor.Composition;
using Microsoft.R.Support.Markdown.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Support.Markdown.Classification
{
    /// <summary>
    /// Implements <see cref="IClassifier"/> and provides classification (colorization) of CoffeeScript items
    /// </summary>
    internal sealed class MdClassifier : TokenBasedClassifier<MdTokenType, MdToken>
    {
        public MdClassifier(ITextBuffer textBuffer,
                            IClassificationTypeRegistryService classificationRegistryService,
                            IContentTypeRegistryService contentTypeRegistryService,
                            IEnumerable<Lazy<IClassificationNameProvider, IComponentContentTypes>> classificationNameProviders) :
            base(textBuffer, new MdTokenizer(), new MdClassificationNameProvider())
        {
            ContentTypeRegistryService = contentTypeRegistryService;
            ClassificationNameProviders = classificationNameProviders;
            ClassificationRegistryService = classificationRegistryService;
        }


        protected override void RemoveSensitiveTokens(int position, TextRangeCollection<MdToken> tokens)
        {
            for (int i = tokens.Count - 1; i >= 1; i--)
            {
                if (tokens[i] is MdRCodeToken)
                {
                    tokens.RemoveRange(i - 1, 2);
                    break;
                }

                if (tokens[i].TokenType == MdTokenType.Code && tokens[i - 1] is MdRCodeToken)
                {
                    tokens.RemoveRange(i - 2, 3);
                    break;
                }
            }

            base.RemoveSensitiveTokens(position, tokens);
        }
    }
}
