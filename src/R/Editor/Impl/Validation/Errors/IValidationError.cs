// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Validation.Errors {
    /// <summary>
    /// Represents validation result. May be based
    /// on AST node or a standalone token.
    /// </summary>
    public interface IValidationError: ITextRange
    {
        /// <summary>
        /// Message to place in a task list and/or tooltip
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Location of the error in the element.
        /// </summary>
        ErrorLocation Location { get; }

        /// <summary>
        /// Error severity
        /// </summary>
        ErrorSeverity Severity { get; }
    }
}
