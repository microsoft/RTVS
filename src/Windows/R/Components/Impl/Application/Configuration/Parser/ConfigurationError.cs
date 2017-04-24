// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Application.Configuration.Parser {
    public sealed class ConfigurationError {
        public int LineNumber { get; }
        public string Message { get; }

        internal ConfigurationError(int lineNumber, string message) {
            LineNumber = lineNumber;
            Message = message;
        }
    }
}
