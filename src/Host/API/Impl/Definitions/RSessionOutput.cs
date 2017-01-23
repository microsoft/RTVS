// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.R.Host.Client {
    public sealed class RSessionOutput {
        public string Output { get; }
        public string Errors { get; }

        internal RSessionOutput(string output, string errors) {
            Output = output;
            Errors = errors;
        }
    }
}
