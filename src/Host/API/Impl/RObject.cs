// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client.API {
    public sealed class RObject {
        public string Name { get; }
        public RObject(string name) {
            Name = name;
        }
    }
}
