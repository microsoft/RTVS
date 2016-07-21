// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    public class BlobFileInfo : IRBlobFileInfo {
        private readonly long _id;
        private readonly string _fileName;
        private readonly long _size;

        private BlobFileInfo(long id, string fileName, long size) {
            _id = id;
            _fileName = fileName;
            _size = size;
        }

        public string FileName {
            get {
                return _fileName;
            }
        }

        public long Size {
            get {
                return _size;
            }
        }

        public long Id {
            get {
                return _id;
            }
        }

        public static BlobFileInfo Create(JToken jToken) {
            JObject jsonObj = (JObject)jToken;
            long id = jsonObj["blob_id"].Value<long>();
            string fileName = jsonObj["file_name"].Value<string>();

            JObject fileInfo = (JObject)jsonObj["file_info"].Value<JObject>();
            long size = fileInfo["size"].Value<long>();
            return new BlobFileInfo(id, fileName, size);
        }
    }
}
