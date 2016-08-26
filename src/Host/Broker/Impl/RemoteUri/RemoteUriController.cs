// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Protocol;
using Microsoft.R.Host.Broker.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.IO;

namespace Microsoft.R.Host.Broker.RemoteUri {

    [Authorize(Policy = Policies.RUser)]
    [Route("/remoteuri")]
    public class RemoteUriController : Controller {
        [HttpPost]
        public async Task<RemoteUriResponse> PostAsync([FromBody] RemoteUriRequest request) {
            HttpClient client = new HttpClient();
            Uri uri = new Uri(request.Uri);

            HttpMethod method = new HttpMethod(request.Method);
            HttpRequestMessage req = new HttpRequestMessage(method, uri);

            foreach (var key in request.Headers.Keys) {
                var val = request.Headers[key];
                req.Headers.Add(key, val);
            }

            if(method == HttpMethod.Post) {
                req.Content = new StringContent(request.Content);
            }
            
            var httpResponse = await client.SendAsync(req);
            var response = await RemoteUriResponse.CreateAsync(httpResponse);

            return response;
        }
    }
}
