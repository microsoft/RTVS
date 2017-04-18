// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public class REvaluationException : RException {
        public REvaluationException(string message)
            : base(message) {
        }
    }
}