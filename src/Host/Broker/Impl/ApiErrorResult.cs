// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker {
    internal sealed class ApiErrorResult : ObjectResult {
        public ApiErrorResult(HttpResponse response, BrokerApiError error, string message = null) : base(error) {
            // https://tools.ietf.org/html/rfc7231#section-6.5.1
            // The 400(Bad Request) status code indicates that the server cannot or 
            // will not process the request due to something that is perceived to be 
            // a client error(e.g., malformed request syntax, invalid request message 
            // framing, or deceptive request routing).
            StatusCode = StatusCodes.Status400BadRequest;

            var headers = response.Headers;
            headers.Add(CustomHttpHeaders.RTVSApiError, new Extensions.Primitives.StringValues(error.ToString()));
            if (!string.IsNullOrEmpty(message)) {
                headers.Add(CustomHttpHeaders.RTVSBrokerException, message);
            }
        }
    }
}
