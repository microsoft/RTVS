// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client.API {
    public sealed class RFunctionArg {
        public string Name { get; }
        public object Value { get; }

        public RFunctionArg(string value) : this(null, value) { }

        public RFunctionArg(string name, string value) {
            Name = name;
            Value = value;
        }
    }
}
