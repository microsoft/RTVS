// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Plots {
    public sealed class RPlotManagerException : Exception {
        public RPlotManagerException(string message, Exception innerException = null)
            : base(message, innerException) {
        }
    }
}
