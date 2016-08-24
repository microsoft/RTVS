// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Broker.Protocol;
using Microsoft.R.Host.Broker.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace Microsoft.R.Host.Broker.RemoteUri {

    [Authorize(Policy = Policies.RUser)]
    [Route("/remoteuri")]
    public class RemoteUriController : Controller, IRemoteUriWebService {
        [HttpPost]
        public async Task<RemoteUriResponse> PostAsync([FromBody] RemoteUriRequest request) {
            HttpClient client = new HttpClient();
            string uri = JsonConvert.DeserializeObject<string>(request.Uri);

            HttpRequestMessage req = new HttpRequestMessage(new HttpMethod(request.Method), uri);
            foreach(var key in request.Headers.AllKeys) {
                var val = request.Headers[key];
                req.Headers.Add(key, val);
            }

            req.Content = new StringContent(JsonConvert.DeserializeObject<string>(request.Content));
            var response = await client.SendAsync(req);

            return RemoteUriResponse.Create(response);
        }
    }
}
