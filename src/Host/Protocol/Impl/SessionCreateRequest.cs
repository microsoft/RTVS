// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Microsoft.R.Host.Protocol {
    public struct SessionCreateRequest {
        public string InterpreterId { get; set; }

        public string CommandLineArguments { get; set; }
    }

    public class RemoteUriRequest {
        public string Method { get; set; }
        public string Uri { get; set; }
        public Dictionary<string,string> Headers { get; set; }
        public string Content { get; set; }

        public static RemoteUriRequest Create(HttpListenerRequest request, string ip, int port) {
            UriBuilder ub = new UriBuilder(request.Url);
            string localHostPort = $"{ub.Host}:{ub.Port}";
            string remoteHostPort = $"{ip}:{port}";
            ub.Host = ip;
            ub.Port = port;
            
            RemoteUriRequest req = new RemoteUriRequest();
            req.Uri = ub.Uri.ToString();
            req.Headers = new Dictionary<string, string>();

            foreach(string key in request.Headers.AllKeys) {
                string value = request.Headers[key];
                value = value.Replace(localHostPort, remoteHostPort);
                req.Headers.Add(key, value);
            }

            req.Method = request.HttpMethod;

            StreamReader reader = new StreamReader(request.InputStream);
            string content = reader.ReadToEnd();
            req.Content = content;

            return req;
        }
    }
}
