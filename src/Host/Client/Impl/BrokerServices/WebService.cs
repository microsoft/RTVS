// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class WebService {
        protected HttpClient HttpClient { get; }

        public WebService(HttpClient httpClient) {
            HttpClient = httpClient;
        }

        private static HttpResponseMessage EnsureSuccessStatusCode(HttpResponseMessage response) {
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden) {
                throw new UnauthorizedAccessException();
            }

            return response.EnsureSuccessStatusCode();
        }

        public async Task<TResponse> HttpGetAsync<TResponse>(Uri uri) =>
            JsonConvert.DeserializeObject<TResponse>(await HttpClient.GetStringAsync(uri));

        public Task<TResponse> HttpGetAsync<TResponse>(UriTemplate uriTemplate, params object[] args) =>
           HttpGetAsync<TResponse>(MakeUri(uriTemplate, args));

        public async Task HttpPutAsync<TRequest>(Uri uri, TRequest request) {
            var requestBody = JsonConvert.SerializeObject(request);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            EnsureSuccessStatusCode(await HttpClient.PutAsync(uri, content));
        }

        public Task HttpPutAsync<TRequest>(UriTemplate uriTemplate, TRequest request, params object[] args) =>
            HttpPutAsync(MakeUri(uriTemplate, args), request);

        public async Task<TResponse> HttpPutAsync<TRequest, TResponse>(Uri uri, TRequest request) {
            var requestBody = JsonConvert.SerializeObject(request);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = EnsureSuccessStatusCode(await HttpClient.PutAsync(uri, content));
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResponse>(responseBody);
        }

        public async Task<Stream> HttpPostAsync(Uri uri, Stream request) {
            var content = new StreamContent(request); // new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = (await HttpClient.PostAsync(uri, content)).EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }

        public Task<TResponse> HttpPutAsync<TRequest, TResponse>(UriTemplate uriTemplate, TRequest request, params object[] args) =>
            HttpPutAsync<TRequest, TResponse>(MakeUri(uriTemplate, args), request);

        private Uri MakeUri(UriTemplate uriTemplate, params object[] args) =>
            uriTemplate.BindByPosition(HttpClient.BaseAddress, args.Select(x => x.ToString()).ToArray());
    }
}
