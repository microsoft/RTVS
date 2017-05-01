// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Classification;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Classification.MD {
    /// <summary>
    /// Implements <see cref="IClassifier"/> and provides classification (colorization) of CoffeeScript items
    /// </summary>
    internal sealed class MdClassifier : TokenBasedClassifier<MarkdownTokenType, MarkdownToken> {
        public MdClassifier(ITextBuffer textBuffer,
                            IClassificationTypeRegistryService classificationRegistryService,
                            IContentTypeRegistryService contentTypeRegistryService) :
            base(textBuffer, new MdTokenizer(), new MarkdownClassificationNameProvider()) {
            ContentTypeRegistryService = contentTypeRegistryService;
            ClassificationRegistryService = classificationRegistryService;
        }


        protected override void RemoveSensitiveTokens(int position, TextRangeCollection<MarkdownToken> tokens) {
            // Check if change damages code block. Normally base classifier removes all tokens
            // from the caret position to the end of the visible area. If typing is inside
            // the code area it may also affects tokens starting from the beginning of the code
            // block. For example, when user types ``` in a middle of existing ```...``` block. 
            // This is similar to typing %> or ?> in a middle of ASP.NET or PHP block.
            int last = tokens.Count - 1;
            if (last >= 0 && tokens[last].TokenType == MarkdownTokenType.Code) {
                tokens.RemoveAt(last);
            }

            base.RemoveSensitiveTokens(position, tokens);
        }
    }
}
