// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Validation.Errors;

namespace Microsoft.R.Editor.Validation.Lint {
    internal sealed partial class LintValidator : IRDocumentValidator {
        // https://cran.rstudio.com/web/packages/lintr/lintr.pdf

        private static readonly Func<IAstNode, LintOptions, IValidationError>[] _singleCheckers =
            new Func<IAstNode, LintOptions, IValidationError>[] {
                AssignmentCheck,
                CommaSpacesCheck,
                InfixOperatorsSpacesCheck,
                OpenCurlyPositionCheck,
                DoubleQuotesCheck,
                SpaceBeforeOpenBraceCheck,
                CloseCurlySeparateLineCheck,
                SpacesInsideParenthesisCheck,
                SpaceAfterFunctionNameCheck,
                SemicolonCheck,
                MultipleStatementsCheck,
                TrueFalseNamesCheck
            };

        private static readonly Func<IAstNode, LintOptions, IEnumerable<IValidationError>>[] _multipleCheckers =
            new Func<IAstNode, LintOptions, IEnumerable<IValidationError>>[] {
                NameCheck
            };

        private static readonly Func<CharacterStream, LintOptions, IValidationError>[] _whitespaceCharCheckers =
            new Func<CharacterStream, LintOptions, IValidationError>[] {
                TabCheck,
                TrailingWhitespaceCheck,
            };

        private static readonly Func<ITextProvider, LintOptions, IEnumerable<IValidationError>>[] _whitespaceFileCheckers =
            new Func<ITextProvider, LintOptions, IEnumerable<IValidationError>>[] {
                TrailingBlankLinesCheck,
                LineLengthCheck
            };

        private IREditorSettings _settings;

        public void OnBeginValidation(IREditorSettings settings) {
            _settings = settings;
        }

        public void OnEndValidation() { }

        public IReadOnlyCollection<IValidationError> ValidateElement(IAstNode node) {
            if (!_settings.LintOptions.Enabled) {
                return Enumerable.Empty<IValidationError>().ToList();
            }

            var warnings = new List<IValidationError>();
            // Unrolled since most return nulls.
            foreach (var c in _singleCheckers) {
                var result = c(node, _settings.LintOptions);
                if (result != null) {
                    warnings.Add(result);
                }
            }

            warnings.AddRange(_multipleCheckers.SelectMany(m => m(node, _settings.LintOptions)));
            return warnings;
        }

        /// <summary>
        /// Checks file whitespace (typically Lint-type or style type checkers.
        /// </summary>
        /// <returns>A collection of validation errors</returns>
        public IReadOnlyCollection<IValidationError> ValidateWhitespace(ITextProvider tp) {
            if (!_settings.LintOptions.Enabled) {
                return Enumerable.Empty<IValidationError>().ToList();
            }

            var warnings = _whitespaceFileCheckers.SelectMany(c => c(tp, _settings.LintOptions)).ToList();
            var cs = new CharacterStream(tp);
            while (!cs.IsEndOfStream()) {
                if (cs.IsWhiteSpace()) {
                    // Unrolled since most return nulls.
                    foreach (var c in _whitespaceCharCheckers) {
                        var result = c(cs, _settings.LintOptions);
                        if (result != null) {
                            warnings.Add(result);
                        }
                    }
                }
                cs.MoveToNextChar();
            }
            return warnings.ToList();
        }
    }
}
