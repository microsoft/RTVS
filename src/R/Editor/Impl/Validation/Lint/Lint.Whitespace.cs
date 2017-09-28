// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Validation.Errors;

namespace Microsoft.R.Editor.Validation.Lint {
    internal partial class LintValidator {
        private static IValidationError TabCheck(CharacterStream cs, ILintOptions options) {
            if (options.NoTabs && cs.CurrentChar == '\t' && cs.Position < cs.Length) {
                // // no_tab_linter: check that only spaces are used, never tabs
                return new ValidationWarning(new TextRange(cs.Position, 1), Resources.Lint_Tabs, ErrorLocation.Token);
            }
            return null;
        }

        private static IValidationError TrailingWhitespaceCheck(CharacterStream cs, ILintOptions options) {
            if (options.TrailingWhitespace) {
                if (cs.IsWhiteSpace() && !cs.CurrentChar.IsLineBreak() && (cs.NextChar.IsLineBreak() || cs.Position == cs.Length - 1)) {
                    // trailing_whitespace_linter: check there are no trailing whitespace characters.
                    return new ValidationWarning(new TextRange(cs.Position, 1), Resources.Lint_TrailingWhitespace, ErrorLocation.Token);
                }
            }
            return null;
        }

        private static IEnumerable<IValidationError> TrailingBlankLinesCheck(ITextProvider tp, ILintOptions options, bool projectedBuffer) {
            if (options.TrailingBlankLines && !projectedBuffer && tp.Length > 1) {
                // trailing_blank_lines_linter: check there are no trailing blank lines
                var trailingWhitespace = string.Empty;
                int i;
                for (i = tp.Length - 1; i >= 0; i--) {
                    if (!char.IsWhiteSpace(tp[i]) && i < tp.Length - 1) {
                        i++;
                        trailingWhitespace = tp.GetText(new TextRange(i, tp.Length - i));
                        break;
                    }
                }
                // On Windows, we get \r\n and need to squiggle \r.
                // On other platforms we may get standalone \n and should then report it
                var warnings = GetEolWarnings(trailingWhitespace, i, '\r');
                if (!warnings.Any()) {
                    warnings = GetEolWarnings(trailingWhitespace, i, '\n');
                }
                return warnings;
            }
            return Enumerable.Empty<IValidationError>();
        }

        private static IEnumerable<IValidationError> GetEolWarnings(string trailingWhitespace, int baseIndex, char eol) {
            if (trailingWhitespace.Count(ch => ch == eol) > 1) {
                // Squiggle all extra blank lines (a single one is OK)
                return trailingWhitespace
                        .IndexWhere(ch => ch == eol)
                        .Skip(1)
                        .Select(x =>
                            new ValidationWarning(new TextRange(baseIndex + x, 1), Resources.Lint_TrailingBlankLines, ErrorLocation.Token));
            }
            return Enumerable.Empty<IValidationError>();
        }

        private static IEnumerable<IValidationError> LineLengthCheck(ITextProvider tp, ILintOptions options, bool projectedBuffer) {
            if (!options.LineLength || tp.Length <= 1) {
                return Enumerable.Empty<IValidationError>();
            }
            // line_length_linter: check the line length of both comments and code is less than length.
            var list = new List<IValidationError>();
            var start = 0;
            for (var i = 0; i < tp.Length + 1; i++) {
                var ch = i < tp.Length ? tp[i] : '\0';
                if (ch.IsLineBreak() || ch == '\0') {
                    var length = i - start;
                    if (length > options.MaxLineLength) {
                        list.Add(new ValidationWarning(new TextRange(start, length), Resources.Lint_LineTooLong.FormatInvariant(length, options.MaxLineLength), ErrorLocation.Token));
                    }

                    if (i < tp.Length && ((ch == '\r' && tp[i + 1] == '\n') || (ch == '\n' && tp[i + 1] == '\r'))) {
                        i++;
                    }
                    start = i + 1;
                }
            }
            return list;
        }
    }
}
