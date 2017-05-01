// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.PackageManager {
    public sealed class RPackageManagerException : Exception {
        public RPackageManagerException(string message, Exception innerException = null)
            : base(message, innerException) {
        }
    }
}
