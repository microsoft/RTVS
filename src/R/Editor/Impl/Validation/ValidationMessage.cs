// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Validation.Errors {
    public class ValidationMessage : ValidationErrorBase {
        public ValidationMessage(ITextRange range, string message, ErrorLocation location) :
            base(range, message, location, ErrorSeverity.Informational) {
        }
    }
}
