// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public class RBlobInfo : IRBlobInfo {
        private readonly ulong _id;

        public RBlobInfo(ulong id) {
            _id = id;
        }

        public ulong Id => _id;
    }
}
