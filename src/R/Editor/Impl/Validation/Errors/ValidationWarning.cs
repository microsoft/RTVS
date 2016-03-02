// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationWarning : ValidationErrorBase
    {
        /// <summary>
        /// Constructs validation warning for an element at a specified location.
        /// </summary>
        public ValidationWarning(ITextRange range, string message, ErrorLocation location) :
             base(range, message, location, ErrorSeverity.Warning)
        {
        }
    }
}
