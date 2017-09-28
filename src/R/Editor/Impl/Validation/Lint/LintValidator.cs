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

        private static readonly Func<IAstNode, ILintOptions, bool, IValidationError>[] _singleCheckers = {
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

        private static readonly Func<IAstNode, ILintOptions, IEnumerable<IValidationError>>[] _multipleCheckers = {
                NameCheck
            };

        private static readonly Func<CharacterStream, ILintOptions, IValidationError>[] _whitespaceCharCheckers = {
                TabCheck,
                TrailingWhitespaceCheck
            };

        private static readonly Func<ITextProvider, ILintOptions, bool, IEnumerable<IValidationError>>[] _whitespaceFileCheckers = {
                TrailingBlankLinesCheck,
                LineLengthCheck
            };

        private IREditorSettings _settings;
        private bool _projectedBuffer;
        private bool _linterEnabled;

        public void OnBeginValidation(IREditorSettings settings, bool projectedBuffer, bool linterEnabled) {
            _settings = settings;
            _projectedBuffer = projectedBuffer;
            _linterEnabled = linterEnabled & settings.LintOptions.Enabled;
        }

        public void OnEndValidation() { }

        public IReadOnlyCollection<IValidationError> ValidateElement(IAstNode node) {
            if (!_linterEnabled) {
                return Enumerable.Empty<IValidationError>().ToList();
            }

            var warnings = _singleCheckers
                .Select(c => c(node, _settings.LintOptions, _projectedBuffer))
                .Where(result => result != null).ToList();

            warnings.AddRange(_multipleCheckers.SelectMany(m => m(node, _settings.LintOptions)));
            return warnings;
        }

        /// <summary>
        /// Checks file whitespace (typically Lint-type or style type checkers.
        /// </summary>
        /// <returns>A collection of validation errors</returns>
        public IReadOnlyCollection<IValidationError> ValidateWhitespace(ITextProvider tp) {
            if (!_linterEnabled) {
                return Enumerable.Empty<IValidationError>().ToList();
            }

            var warnings = _whitespaceFileCheckers
                            .SelectMany(c => c(tp, _settings.LintOptions, _projectedBuffer))
                            .ToList();

            var cs = new CharacterStream(tp);
            while (!cs.IsEndOfStream()) {
                if (cs.IsWhiteSpace()) {
                    // Unrolled since most return nulls.
                    warnings.AddRange(_whitespaceCharCheckers
                                        .Select(c => c(cs, _settings.LintOptions))
                                        .Where(result => result != null));
                }
                cs.MoveToNextChar();
            }
            return warnings.ToList();
        }
    }
}
