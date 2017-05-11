// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Validation.Errors {
    public class ValidationSentinel : ValidationErrorBase {
        /// <summary>
        /// Constructs 'barrier' pseudo error that clears all messages for a given node.
        /// </summary>
        public ValidationSentinel() :
            base(TextRange.EmptyRange, String.Empty, ErrorLocation.Token, ErrorSeverity.Error) {
        }
    }
}
