// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Validation.Errors;

namespace Microsoft.R.Editor.Validation {
    /// <summary>
    /// AST node validator
    /// </summary>
    public interface IRDocumentValidator {
        /// <summary>
        /// Called by validation manager when validation session is about to begin.
        /// </summary>
        void OnBeginValidation(IREditorSettings settings, bool projectedBuffer, bool linterEnabled);

        /// <summary>
        /// Called by validation manager/aggregator when validation session is completed.
        /// </summary>
        void OnEndValidation();

        /// <summary>
        /// Validates a single AST node element.
        /// </summary>
        /// <returns>A collection of validation errors</returns>
        IReadOnlyCollection<IValidationError> ValidateElement(IAstNode node);
        
        /// <summary>
        /// Checks file whitespace (typically Lint-type or style type checkers.
        /// </summary>
        /// <returns>A collection of validation errors</returns>
        IReadOnlyCollection<IValidationError> ValidateWhitespace(ITextProvider text);
    }
}
