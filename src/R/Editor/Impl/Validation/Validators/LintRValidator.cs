// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Validation.Errors;

namespace Microsoft.R.Editor.Validation.Validators {
    internal sealed class LintRValidator : IValidator {
        // https://cran.rstudio.com/web/packages/lintr/lintr.pdf

        private static readonly Func<IAstNode, IValidationError>[] _checkers = new Func<IAstNode, IValidationError>[] {
                AssignmentCheck,
                CommaSpacesCheck,
            };

        public void OnBeginValidation() { }
        public void OnEndValidation() { }

        public IReadOnlyCollection<IValidationError> ValidateElement(IAstNode node)
            => _checkers.Select(c => c(node)).ExcludeDefault().ToList();

        private static IValidationError AssignmentCheck(IAstNode node) {
            // assignment_linter: checks that ’<-’ is always used for assignment
            if (node is IOperator op && op.OperatorType == OperatorType.Equals) {
                if (!(op.RightOperand is NamedArgument)) {
                    return new ValidationWarning(node, Resources.LintR_Assignment, ErrorLocation.Token);
                }
            }
            return null;
        }

        private static IValidationError CommaSpacesCheck(IAstNode node) {
            // commas_linter: check that all commas are followed by spaces, but do not have spaces before them
            var tp = node.Root.TextProvider;
            bool warning = false;

            if (node is TokenNode && node.Length == 1 && tp.GetText().EqualsOrdinal(",")) {
                warning = node.Start > 0 && char.IsWhiteSpace(tp.GetText(new TextRange(node.Start - 1, 1))[0]);
                if (!warning) {
                    warning = node.End < tp.Length && !char.IsWhiteSpace(tp.GetText(new TextRange(node.End, 1))[0]);
                }
            }
            return warning ? new ValidationWarning(node, Resources.LintR_CommaSpaces, ErrorLocation.Token) : null;
        }
    }
}
