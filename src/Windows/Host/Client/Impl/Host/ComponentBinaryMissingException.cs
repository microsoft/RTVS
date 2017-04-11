// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client {
    public class ComponentBinaryMissingException : Exception {
        public ComponentBinaryMissingException(string name)
            : base(Resources.Error_BinaryMissing15.FormatInvariant(name)) { }
    }
}
