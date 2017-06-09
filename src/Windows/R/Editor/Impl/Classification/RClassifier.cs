// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Classification;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.R.Editor.Classification {
    /// <summary>
    /// Implements <see cref="IClassifier"/> and provides classification 
    /// (colorization) of R language tokens
    /// </summary>
    internal sealed class RClassifier : TokenBasedClassifier<RTokenType, RToken> {
        public RClassifier(ITextBuffer textBuffer, IClassificationTypeRegistryService classificationRegistryService) :
            base(textBuffer, new RTokenizer(), new RClassificationNameProvider()) {
            ClassificationRegistryService = classificationRegistryService;
        }

        public static RClassifier FromTextBuffer(ITextBuffer textBuffer)
            => textBuffer.Properties.TryGetProperty(typeof(RClassifier), out object instance) ? instance as RClassifier : null;

        protected override void RemoveSensitiveTokens(int position, TextRangeCollection<RToken> tokens) {
            if (tokens == null) {
                return;
            }

            while (tokens.Count > 0) {
                int count = tokens.Count;
                var token = tokens[count - 1];

                if ((token.TokenType == RTokenType.Number || token.TokenType == RTokenType.Complex) && token.End + 2 >= position) {
                    // This handles case when user types 1.23e1. In 1.23e case 'e' is not part 
                    // of the number since 1.23e is not a valid js number. However, 1.23e+1 is 
                    // a valid number. Hence we are considering typing within 2 character of 
                    // a number token to be  a sensitive change.
                    tokens.RemoveAt(count - 1);
                    continue;
                }

                if (count > 1) {
                    if (token.Start != tokens[count - 2].End) {
                        break;
                    }

                    tokens.RemoveAt(count - 1);
                    continue;
                }

                break;
            }

            base.RemoveSensitiveTokens(position, tokens);
        }
    }
}
