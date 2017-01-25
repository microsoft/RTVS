// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Describes session output
    /// </summary>
    public sealed class RSessionOutput {
        /// <summary>
        /// Result of the expression execution as it would appear 
        /// in the R console: such as '[1] 2' when '1+1' is executed.
        /// </summary>
        public string Output { get; }

        /// <summary>
        /// Error messages
        /// </summary>
        public string Errors { get; }

        internal RSessionOutput(string output, string errors) {
            Output = output;
            Errors = errors;
        }
    }
}
