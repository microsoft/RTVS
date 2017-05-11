// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Validation.Errors {
    public class ValidationErrorCollection : List<IValidationError> {
        public void Add(ITextRange range, string message, ErrorLocation location) {
            Add(new ValidationError(range, message, location));
        }
    }
}
