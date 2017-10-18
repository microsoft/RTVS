// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Containers {
    public class ContainerServicePermissionException : ContainerException {
        public ContainerServicePermissionException() { }

        public ContainerServicePermissionException(string message) : base(message) { }

        public ContainerServicePermissionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
