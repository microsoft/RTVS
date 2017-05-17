// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.R.Editor.Classification {
    /// <summary>
    /// Implements <see cref="IClassifier"/> and provides classification (colorization) 
    /// of Roxygen items in R comments
    /// </summary>
    internal sealed class RoxygenClassifier : IClassifier {
        private readonly ITextBuffer _textBuffer;
        private readonly IClassificationTypeRegistryService _ctrs;
        private readonly RClassifier _rClassifier;
        private readonly IClassificationType _commentType;

        public RoxygenClassifier(ITextBuffer textBuffer, IClassificationTypeRegistryService ctrs) {
            _textBuffer = textBuffer;
            _ctrs = ctrs;
            _rClassifier = RClassifier.FromTextBuffer(textBuffer);
            _commentType = _ctrs.GetClassificationType(PredefinedClassificationTypeNames.Comment);
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) {
            var rClassificationSpans = _rClassifier.GetClassificationSpans(span);
            return rClassificationSpans
                    .Where(s => s.ClassificationType == _commentType)
                    .SelectMany(s => ClassifyCommentSpan(s))
                    .ToList();
        }

        private IEnumerable<ClassificationSpan> ClassifyCommentSpan(ClassificationSpan cs) {
            var commentText = _textBuffer.CurrentSnapshot.GetText(cs.Span);

            // #' @export aaa bbb
            // #' - remains comment
            // @export - classify as keyword. Only first one matters;
            // aaa - classify as value/constant ONLY after @export
            // Leave everything else as a comment
            var tokenizer = new RoxygenTokenizer();
            var tokens = tokenizer.Tokenize(commentText);
            return tokens.Select(t =>
                new ClassificationSpan(new SnapshotSpan(_textBuffer.CurrentSnapshot, new Span(cs.Span.Start + t.Start, t.Length)),
                                       _ctrs.GetClassificationType(GetClassificationName(t)))).ToList();
        }

        private string GetClassificationName(RToken t) {
            switch (t.TokenType) {
                case RTokenType.Keyword:
                    return ClassificationDefinitions.RoxygenKeywordClassificationFormatName;
                case RTokenType.Identifier:
                    return ClassificationDefinitions.RoxygenExportClassificationFormatName;
            }
            return string.Empty;
        }

        class RoxygenTokenizer : BaseTokenizer<RToken> {
            public override void AddNextToken() {
                if (_cs.CurrentChar == '#' && _cs.NextChar == '\'') {
                    _cs.Advance(2);
                    SkipWhitespace();

                    if (_cs.CurrentChar == '@') {
                        var start = _cs.Position;
                        AddWord(RTokenType.Keyword);
                        TryAddExport(start, _cs.Position - start);
                    }
                } else {
                    _cs.Position = _cs.Length;
                }
            }

            private void TryAddExport(int start, int length) {
                var text = _cs.GetSubstringAt(start, length);
                if (text.EqualsOrdinal("@export")) {
                    AddWord(RTokenType.Identifier);
                }
            }

            private void AddWord(RTokenType type) {
                SkipWhitespace();
                var start = _cs.Position;
                SkipWord();
                if (_cs.Position > start) {
                    AddToken(type, start, _cs.Position - start);
                }
            }

            private void SkipWord() {
                Tokenizer.SkipIdentifier(
                    _cs,
                    (CharacterStream cs) => !cs.IsWhiteSpace(),
                    (CharacterStream cs) => !cs.IsWhiteSpace());
            }

            private void AddToken(RTokenType type, int start, int length)
                => _tokens.Add(new RToken(type, start, length));
        }

#pragma warning disable 67
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
    }
}
