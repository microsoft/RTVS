// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Protocol {
    public class SessionInfo {
        public string Id { get; set; }

        public string InterpreterId { get; set; }

        public string CommandLineArguments { get; set; }
    }

    public class RemoteUriResponse {
        public Dictionary<string, string> Headers { get; set; }
        public string Content { get; set; }

        public static async Task<RemoteUriResponse> CreateAsync(HttpResponseMessage response) {
            var resp = new RemoteUriResponse();
            resp.Content = await response.Content.ReadAsStringAsync();
            resp.Headers = new Dictionary<string, string>();

            foreach(var pair in response.Headers) {
                StringBuilder value = new StringBuilder("");
                foreach(var val in pair.Value) {
                    value.Append(val);
                    value.Append(",");
                }
                resp.Headers.Add(pair.Key, value.ToString());
            }


            return resp;
        }
    }
}
