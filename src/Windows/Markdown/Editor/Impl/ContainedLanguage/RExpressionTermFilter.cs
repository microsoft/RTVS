// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Parser;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    /// <summary>
    /// Helps R parser to handle R Markdown-specific constructs
    /// </summary>
    internal sealed class RExpressionTermFilter: IExpressionTermFilter {
        private readonly ITextBuffer _textBuffer;

        public RExpressionTermFilter(ITextBuffer textBuffer) {
            Check.ArgumentNull(nameof(textBuffer), textBuffer);
            _textBuffer = textBuffer; // R language buffer
            _textBuffer.AddService(this);
        }

        /// <summary>
        /// Detemines if particular range should not be treated as contained language and 
        /// instead should be ignored or 'skipped over'. Used by R parser to ignore 'R' in
        /// ```{R ... }
        /// </summary>
        public bool IsInertRange(ITextRange range) {
            // This is a workaround for constructs like ```{r x = 1, y = FALSE} where the { }
            // block is treated as R fragment. The fragment is syntactually incorrect since
            // 'r' is indentifier and there is an operator expected between 'r' and 'x'.
            // In order to avoid parsing errors expression parser will use this flag and
            // allow standalone indentifier 'r' or 'R' right after the opening curly brace.
            if (range.Length == 1 && range.Start > 0) {
                // Map range up from contained language
                var view = _textBuffer.GetFirstView();
                var viewPoint = view?.MapUpToView(new SnapshotPoint(_textBuffer.CurrentSnapshot, range.Start));
                if (viewPoint.HasValue) {
                    var snapshot = view.TextBuffer.CurrentSnapshot;
                    if (snapshot.Length >= 2 && snapshot.GetText(viewPoint.Value - 1, 2).EqualsIgnoreCase("`r")) {
                        return true;
                    }
                    if (snapshot.Length >= 5 && viewPoint.Value > 3 && snapshot.GetText(viewPoint.Value - 4, 5).EqualsIgnoreCase("```{r")) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
