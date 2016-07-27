// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public class Blob : IRBlob {
        private readonly ulong _id;
        private readonly byte[] _data;
        
        public Blob(ulong id, byte[] data) {
            _id = id;
            _data = data;
        }

        public byte[] Data => _data;
        public ulong Id => _id;
    }
}
