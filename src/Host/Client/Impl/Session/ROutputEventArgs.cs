// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client {
    public class ROutputEventArgs : EventArgs {
        public OutputType OutputType { get; }
        public string Message { get; }

        public ROutputEventArgs(OutputType outputType, string message) {
            OutputType = outputType;
            Message = message;
        }
    }
}