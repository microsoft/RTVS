// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Validation.Errors;

namespace Microsoft.R.Editor.Validation.LintR {
    internal partial class LintValidator {
        private static IValidationError TabCheck(CharacterStream cs, LintOptions options) {
            if (options.NoTabs && cs.CurrentChar == '\t' && cs.Position < cs.Length) {
                // // no_tab_linter: check that only spaces are used, never tabs
                return new ValidationWarning(new TextRange(cs.Position, 1), Resources.Lint_Tabs, ErrorLocation.Token);
            }
            return null;
        }

        private static IValidationError TrailingWhitespaceCheck(CharacterStream cs, LintOptions options) {
            if (options.TrailingWhitespace && cs.IsWhiteSpace() && cs.NextChar.IsLineBreak()) {
                // trailing_whitespace_linter: check there are no trailing whitespace characters.
                return new ValidationWarning(new TextRange(cs.Position, 1), Resources.Lint_TrailingWhitespace, ErrorLocation.Token);
            }
            return null;
        }

        private static IEnumerable<IValidationError> TrailingBlankLinesCheck(ITextProvider tp, LintOptions options) {
            if (options.TrailingBlankLines && tp.Length > 1) {
                // trailing_blank_lines_linter: check there are no trailing blank lines
                var trailingWhitespace = string.Empty;
                int i;
                for (i = tp.Length - 1; i >= 0; i--) {
                    if (!char.IsWhiteSpace(tp[i]) && i < tp.Length - 1) {
                        trailingWhitespace = tp.GetText(new TextRange(i + 1, tp.Length - i));
                        break;
                    }
                }
                if (trailingWhitespace.Count(ch => ch == '\n') > 1 || trailingWhitespace.Count(ch => ch == '\r') > 1) {
                    return new[] { new ValidationWarning(new TextRange(i + 1, 1), Resources.Lint_TrailingBlankLines, ErrorLocation.Token) };
                }
            }
            return Enumerable.Empty<IValidationError>();
        }

        private static IEnumerable<IValidationError> LineLengthCheck(ITextProvider tp, LintOptions options) {
            if (options.LineLength && tp.Length > 1) {
                // line_length_linter: check the line length of both comments and code is less than length.
                var list = new List<IValidationError>();                var start = 0;
                for (var i = 0; i < tp.Length + 1; i++) {
                    var ch = i < tp.Length ? tp[i] : '\0';
                    if (ch == '\r' || ch == '\n' || ch == '\0') {
                        var length = i - start;
                        if (length > options.MaxLineLength) {
                            list.Add(new ValidationWarning(new TextRange(start, 1), Resources.Lint_LineTooLong.FormatInvariant(length, options.MaxLineLength), ErrorLocation.Token));
                        }
                        start = i;
                    }
                }
            }
            return Enumerable.Empty<IValidationError>();
        }
    }
}
