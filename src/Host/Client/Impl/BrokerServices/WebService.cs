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
        private readonly ICredentialsDecorator _credentialsDecorator;

        protected HttpClient HttpClient { get; }

        public WebService(HttpClient httpClient, ICredentialsDecorator credentialsDecorator) {
            HttpClient = httpClient;
            _credentialsDecorator = credentialsDecorator;
        }

        private static HttpResponseMessage EnsureSuccessStatusCode(HttpResponseMessage response) {
            try {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden) {
                    throw new UnauthorizedAccessException();
                }

                IEnumerable<string> values;
                if (response.Headers.TryGetValues(CustomHttpHeaders.RTVSApiError, out values)) {
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
            } catch {
                response.Dispose();
                throw;
            }
        }

        private async Task<T> RepeatUntilAuthenticatedAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken) {
            while (true) {
                using (await _credentialsDecorator.LockCredentialsAsync(cancellationToken)) {
                    try {
                        return await action(cancellationToken);
                    } catch (UnauthorizedAccessException) {
                        _credentialsDecorator.InvalidateCredentials();
                    }
                }
            }
        }

        private Task RepeatUntilAuthenticatedAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
            => RepeatUntilAuthenticatedAsync(async ct => {
                await action(ct);
                return false;
            }, cancellationToken);

        public async Task<TResponse> HttpGetAsync<TResponse>(Uri uri, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var response = await RepeatUntilAuthenticatedAsync(async ct => EnsureSuccessStatusCode(await HttpClient.GetAsync(uri, ct)), cancellationToken)) {
                return JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
            }
        }

        public Task<TResponse> HttpGetAsync<TResponse>(UriTemplate uriTemplate, CancellationToken cancellationToken = default(CancellationToken), params object[] args) =>
           HttpGetAsync<TResponse>(MakeUri(uriTemplate, args), cancellationToken);

        public async Task HttpPutAsync<TRequest>(Uri uri, TRequest request, CancellationToken cancellationToken) {
            var requestBody = JsonConvert.SerializeObject(request);

            await RepeatUntilAuthenticatedAsync(async ct => (await GetHttpPutResponseAsync(uri, requestBody, ct)).Dispose(), cancellationToken);
        }

        public Task HttpPutAsync<TRequest>(UriTemplate uriTemplate, TRequest request, CancellationToken cancellationToken, params object[] args) =>
            HttpPutAsync(MakeUri(uriTemplate, args), request, cancellationToken);

        public async Task<TResponse> HttpPutAsync<TRequest, TResponse>(Uri uri, TRequest request, CancellationToken cancellationToken = default(CancellationToken)) {
            var requestBody = JsonConvert.SerializeObject(request);

            using (var response = await RepeatUntilAuthenticatedAsync(ct => GetHttpPutResponseAsync(uri, requestBody, ct), cancellationToken))  {
                var responseBody = await response.Content.ReadAsStringAsync();
                try {
                    return JsonConvert.DeserializeObject<TResponse>(responseBody);
                } catch (JsonSerializationException ex) {
                    throw new ProtocolViolationException(ex.Message);
                }
            }
        }

        private async Task<HttpResponseMessage> GetHttpPutResponseAsync(Uri uri, string requestBody, CancellationToken cancellationToken) {
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            return EnsureSuccessStatusCode(await HttpClient.PutAsync(uri, content, cancellationToken));
        }

        public Task<TResponse> HttpPutAsync<TRequest, TResponse>(UriTemplate uriTemplate, TRequest request, CancellationToken cancellationToken = default(CancellationToken), params object[] args) =>
            HttpPutAsync<TRequest, TResponse>(MakeUri(uriTemplate, args), request, cancellationToken);

        public async Task<Stream> HttpPostAsync(Uri uri, Stream request, CancellationToken cancellationToken) {
            var content = new StreamContent(request);

            using (var response = await RepeatUntilAuthenticatedAsync(async ct => EnsureSuccessStatusCode(await HttpClient.PostAsync(uri, content, ct)), cancellationToken)) {
                return await response.Content.ReadAsStreamAsync();
            }
        }

        public Task HttpDeleteAsync(Uri uri, CancellationToken cancellationToken = default(CancellationToken)) =>
            RepeatUntilAuthenticatedAsync(async ct => EnsureSuccessStatusCode(await HttpClient.DeleteAsync(uri, ct)).Dispose(), cancellationToken);
        
        public Task HttpDeleteAsync(UriTemplate uriTemplate, CancellationToken cancellationToken = default(CancellationToken), params object[] args) =>
            HttpDeleteAsync(MakeUri(uriTemplate, args), cancellationToken);

        private Uri MakeUri(UriTemplate uriTemplate, params object[] args) =>
            uriTemplate.BindByPosition(HttpClient.BaseAddress, args.Select(x => x.ToString()).ToArray());
    }
}
