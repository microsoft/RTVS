// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Validation.Errors {
    public class ValidationErrorBase : TextRange, IValidationError {
        /// <summary>
        /// Error or warning message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Error location
        /// </summary>
        public ErrorLocation Location { get; private set; }

        /// <summary>
        /// Error severity
        /// </summary>
        public ErrorSeverity Severity { get; private set; }

        public ValidationErrorBase(ITextRange range, string message, ErrorLocation location, ErrorSeverity severity) :
            base(range) {
            Message = message;
            Severity = severity;
            Location = location;
        }

        private static ITextRange GetLocationRange(IAstNode node, RToken token, ErrorLocation location) {
            ITextRange itemRange = node != null ? node : token as ITextRange;

            //switch (location)
            //{
            //    case ErrorLocation.BeforeToken:
            //        return new TextRange(Math.Max(0, itemRange.Start-1), 1);

            //    case ErrorLocation.AfterToken:
            //        return new TextRange(itemRange.End, 1);
            //}

            return itemRange;
        }
    }
}
