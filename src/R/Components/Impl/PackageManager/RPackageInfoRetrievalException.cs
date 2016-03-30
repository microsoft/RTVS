// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Test.PackageManager {
    [Serializable]
    public sealed class RPackageInfoRetrievalException : Exception {
        public RPackageInfoRetrievalException(string message, Exception innerException = null)
            : base(message, innerException) {
        }
    }
}
