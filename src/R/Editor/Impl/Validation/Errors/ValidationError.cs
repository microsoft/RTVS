// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Validation.Errors {
    public class ValidationError : ValidationErrorBase {
        /// <summary>
        /// Constructs validation error based on existing error.
        /// </summary>
        public ValidationError(IValidationError error) :
            base(error, error.Message, error.Location, error.Severity) {
        }

        /// <summary>
        /// Constructs validation error for an element at a specified location.
        /// </summary>
        public ValidationError(ITextRange range, string message, ErrorLocation location) :
            base(range, message, location, ErrorSeverity.Error) {
        }
    }
}
