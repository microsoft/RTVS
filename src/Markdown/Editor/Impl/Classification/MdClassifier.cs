using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Classification;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Classification
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
            int last = tokens.Count - 1;
            if (last >= 0 && tokens[last].TokenType == MdTokenType.Code)
            {
                tokens.RemoveAt(last);
            }

            base.RemoveSensitiveTokens(position, tokens);
        }
    }
}
