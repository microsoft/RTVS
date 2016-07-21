// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client {
    public class BlobData : IRBlobData {
        private readonly long _id;
        private readonly byte[] _data;
        private readonly RBlobKind _kind;

        public BlobData(long id, byte[] data, RBlobKind kind = RBlobKind.Unspecified) {
            _id = id;
            _data = data;
            _kind = kind;
        }

        public long Id {
            get {
                return _id;
            }
        }

        public RBlobKind Kind {
            get {
                return RBlobKind.Unspecified;
            }
        }

        byte[] IRBlobData.Data {
            get {
                return _data;
            }
        }
    }
}
