// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Containers {
    public class ContainerServiceNotInstalledException : ContainerException {
        public ContainerServiceNotInstalledException() { }

        public ContainerServiceNotInstalledException(string message) : base(message) { }

        public ContainerServiceNotInstalledException(string message, Exception innerException) : base(message, innerException) { }
    }
}
