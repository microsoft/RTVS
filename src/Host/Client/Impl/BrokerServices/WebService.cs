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
using Microsoft.Common.Core.Json;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class WebService {
        private readonly ICredentialsDecorator _credentialsDecorator;
        private readonly IActionLog _log;

        protected HttpClient HttpClient { get; }

        public WebService(HttpClient httpClient, ICredentialsDecorator credentialsDecorator, IActionLog log) {
            HttpClient = httpClient;
            _credentialsDecorator = credentialsDecorator;
            _log = log;
        }

        private static HttpResponseMessage EnsureSuccessStatusCode(HttpResponseMessage response) {
            try {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden) {
                    throw new UnauthorizedAccessException();
                }

                if (response.Headers.TryGetValues(CustomHttpHeaders.RTVSApiError, out IEnumerable<string> values)) {
                    var s = values.FirstOrDefault();
                    if (s != null) {
                        if (Enum.TryParse(s, out BrokerApiError apiError)) {
                            response.Headers.TryGetValues(CustomHttpHeaders.RTVSBrokerException, out values);
                            throw new BrokerApiErrorException(apiError, values?.FirstOrDefault());
                        }
                        throw new ProtocolViolationException("Unknown broker API error");
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
                    } catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
                        throw new HttpRequestException(Resources.Error_OperationTimedOut);
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
            using (var response = await RepeatUntilAuthenticatedAsync(async ct => EnsureSuccessStatusCode(await GetAsync(uri, ct)), cancellationToken)) {
                return Json.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task HttpPutAsync<TRequest>(Uri uri, TRequest request, CancellationToken cancellationToken) {
            var requestBody = JsonConvert.SerializeObject(request);

            await RepeatUntilAuthenticatedAsync(async ct => (await GetHttpPutResponseAsync(uri, requestBody, ct)).Dispose(), cancellationToken);
        }


        public async Task<TResponse> HttpPutAsync<TRequest, TResponse>(Uri uri, TRequest request, CancellationToken cancellationToken = default(CancellationToken)) {
            var requestBody = JsonConvert.SerializeObject(request);

            using (var response = await RepeatUntilAuthenticatedAsync(ct => GetHttpPutResponseAsync(uri, requestBody, ct), cancellationToken)) {
                var responseBody = await response.Content.ReadAsStringAsync();
                try {
                    return Json.DeserializeObject<TResponse>(responseBody);
                } catch (JsonSerializationException ex) {
                    throw new ProtocolViolationException(ex.Message);
                }
            }
        }

        private async Task<HttpResponseMessage> GetHttpPutResponseAsync(Uri uri, string requestBody, CancellationToken cancellationToken) {
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            return EnsureSuccessStatusCode(await PutAsync(uri, cancellationToken, content));
        }

        public async Task<Stream> HttpPostAsync(Uri uri, Stream request, CancellationToken cancellationToken) {
            var content = new StreamContent(request);

            using (var response = await RepeatUntilAuthenticatedAsync(async ct => EnsureSuccessStatusCode(await PostAsync(uri, content, ct)), cancellationToken)) {
                return await response.Content.ReadAsStreamAsync();
            }
        }

        public Task HttpDeleteAsync(Uri uri, CancellationToken cancellationToken = default(CancellationToken)) =>
            RepeatUntilAuthenticatedAsync(async ct => EnsureSuccessStatusCode(await DeleteAsync(uri, ct)).Dispose(), cancellationToken);

        private async Task<HttpResponseMessage> GetAsync(Uri uri, CancellationToken ct) {
            using (_log.Measure(LogVerbosity.Traffic, Invariant($"GetAsync({uri})"))) {
                return await HttpClient.GetAsync(uri, ct);
            }
        }

        private async Task<HttpResponseMessage> PutAsync(Uri uri, CancellationToken cancellationToken, StringContent content) {
            using (_log.Measure(LogVerbosity.Traffic, Invariant($"PutAsync({uri})"))) {
                return await HttpClient.PutAsync(uri, content, cancellationToken);
            }
        }

        private async Task<HttpResponseMessage> PostAsync(Uri uri, StreamContent content, CancellationToken ct) {
            using (_log.Measure(LogVerbosity.Traffic, Invariant($"PostAsync({uri})"))) {
                return await HttpClient.PostAsync(uri, content, ct);
            }
        }

        private async Task<HttpResponseMessage> DeleteAsync(Uri uri, CancellationToken ct) {
            using (_log.Measure(LogVerbosity.Traffic, Invariant($"DeleteAsync({uri})"))) {
                return await HttpClient.DeleteAsync(uri, ct);
            }
        }
    }
}
