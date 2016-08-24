// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace Microsoft.R.Host.Protocol {
    public struct SessionCreateRequest {
        public string InterpreterId { get; set; }

        public string CommandLineArguments { get; set; }
    }

    public struct RemoteUriRequest {
        public string Method { get; set; }
        public string Uri { get; set; }
        public NameValueCollection Headers { get; set; }
        public string Content { get; set; }

        public static RemoteUriRequest Create(HttpListenerRequest request, string ip, int port) {
            UriBuilder ub = new UriBuilder(request.Url);
            ub.Host = ip;
            ub.Port = port;
            
            RemoteUriRequest req = new RemoteUriRequest();
            req.Uri = ub.Uri.ToString();
            req.Headers = request.Headers;
            req.Method = request.HttpMethod;

            StreamReader reader = new StreamReader(request.InputStream);
            string content = reader.ReadToEnd();
            req.Content = content;

            return req;
        }
    }
}
