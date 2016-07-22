// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public class Blob : IRBlob {
        private readonly long _id;
        private readonly byte[] _data;
        
        public Blob(long id, byte[] data) {
            _id = id;
            _data = data;
        }

        public byte[] Data => _data;
        public long Id => _id;
    }
}
