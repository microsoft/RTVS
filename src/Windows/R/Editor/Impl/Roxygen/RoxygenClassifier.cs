// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Classification;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.R.Editor.Roxygen {
    /// <summary>
    /// Implements <see cref="IClassifier"/> and provides classification (colorization) 
    /// of Roxygen items in R comments
    /// </summary>
    internal sealed class RoxygenClassifier : IClassifier {
        private readonly ITextBuffer _textBuffer;
        private readonly IClassificationTypeRegistryService _ctrs;
        private readonly IClassificationType _commentType;
        private RClassifier _rClassifier;

        public RoxygenClassifier(ITextBuffer textBuffer, IClassificationTypeRegistryService ctrs) {
            _textBuffer = textBuffer;
            _ctrs = ctrs;
            _rClassifier = RClassifier.FromTextBuffer(textBuffer);
            _commentType = _ctrs.GetClassificationType(PredefinedClassificationTypeNames.Comment);
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) {
            var rClassificationSpans = RClassifier.GetClassificationSpans(span);
            return rClassificationSpans
                    .Where(s => s.ClassificationType == _commentType)
                    .SelectMany(s => ClassifyCommentSpan(s))
                    .ToList();
        }

        private RClassifier RClassifier {
            get {
                _rClassifier = _rClassifier ?? RClassifier.FromTextBuffer(_textBuffer);
                return _rClassifier;
            }
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
                    return RoxygenClassificationDefinitions.RoxygenKeywordClassificationFormatName;
                case RTokenType.Identifier:
                    return RoxygenClassificationDefinitions.RoxygenExportClassificationFormatName;
            }
            return string.Empty;
        }

#pragma warning disable 67
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
    }
}
