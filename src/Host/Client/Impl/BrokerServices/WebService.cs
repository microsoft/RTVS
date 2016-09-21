// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json;

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

            IEnumerable<string> values;
            if(response.Headers.TryGetValues(CustomHttpHeaders.RTVSApiError, out values)) {
                var s = values.FirstOrDefault();
                if (s != null) {
                    BrokerApiError apiError;
                    if (Enum.TryParse(s, out apiError)) {
                        response.Headers.TryGetValues(CustomHttpHeaders.RTVSBrokerException, out values);
                        throw new BrokerApiErrorException(apiError, values?.FirstOrDefault());
                    } else {
                        throw new ProtocolViolationException("Unknown broker API error");
                    }
                }
            }

            return response.EnsureSuccessStatusCode();
        }

        public async Task<TResponse> HttpGetAsync<TResponse>(Uri uri, CancellationToken cancellationToken = default(CancellationToken)) {
            var resonse = await HttpClient.GetAsync(uri, cancellationToken);
            return JsonConvert.DeserializeObject<TResponse>(await resonse.Content.ReadAsStringAsync());
        }

        public Task<TResponse> HttpGetAsync<TResponse>(UriTemplate uriTemplate, CancellationToken cancellationToken = default(CancellationToken), params object[] args) =>
           HttpGetAsync<TResponse>(MakeUri(uriTemplate, args), cancellationToken);

        public async Task HttpPutAsync<TRequest>(Uri uri, TRequest request) {
            var requestBody = JsonConvert.SerializeObject(request);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            EnsureSuccessStatusCode(await HttpClient.PutAsync(uri, content));
        }

        public Task HttpPutAsync<TRequest>(UriTemplate uriTemplate, TRequest request, params object[] args) =>
            HttpPutAsync(MakeUri(uriTemplate, args), request);

        public async Task<TResponse> HttpPutAsync<TRequest, TResponse>(Uri uri, TRequest request, CancellationToken cancellationToken = default(CancellationToken)) {
            var requestBody = JsonConvert.SerializeObject(request);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = EnsureSuccessStatusCode(await HttpClient.PutAsync(uri, content, cancellationToken));
            var responseBody = await response.Content.ReadAsStringAsync();
            try {
                return JsonConvert.DeserializeObject<TResponse>(responseBody);
            } catch(JsonSerializationException ex) {
                throw new ProtocolViolationException(ex.Message);
            }
        }

        public async Task<Stream> HttpPostAsync(Uri uri, Stream request) {
            var content = new StreamContent(request);
            var response = (await HttpClient.PostAsync(uri, content)).EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }

        public Task<TResponse> HttpPutAsync<TRequest, TResponse>(UriTemplate uriTemplate, TRequest request, CancellationToken cancellationToken = default(CancellationToken), params object[] args) =>
            HttpPutAsync<TRequest, TResponse>(MakeUri(uriTemplate, args), request, cancellationToken);

        private Uri MakeUri(UriTemplate uriTemplate, params object[] args) =>
            uriTemplate.BindByPosition(HttpClient.BaseAddress, args.Select(x => x.ToString()).ToArray());
    }
}
