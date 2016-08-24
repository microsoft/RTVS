// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Net.Http;

namespace Microsoft.R.Host.Protocol {
    public class SessionInfo {
        public string Id { get; set; }

        public string InterpreterId { get; set; }

        public string CommandLineArguments { get; set; }
    }

    public struct RemoteUriResponse {
        public NameValueCollection Headers { get; set; }
        public string Content { get; set; }

        public static RemoteUriResponse Create(HttpResponseMessage response) {
            return new RemoteUriResponse();
        }
    }
}
