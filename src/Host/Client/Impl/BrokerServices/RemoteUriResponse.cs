// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class RemoteUriResponse {
        public Dictionary<string, string> Headers { get; set; }
        public Stream Content { get; set; }
    }
}
