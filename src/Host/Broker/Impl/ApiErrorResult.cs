// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker {
    public sealed class ApiErrorResult : ObjectResult {
        private BrokerApiError _brokerApiError;
        private readonly string _message;

        public ApiErrorResult(BrokerApiError error, string message = null) : base(error) {
            // https://tools.ietf.org/html/rfc7231#section-6.5.1
            // The 400(Bad Request) status code indicates that the server cannot or 
            // will not process the request due to something that is perceived to be 
            // a client error(e.g., malformed request syntax, invalid request message 
            // framing, or deceptive request routing).
            StatusCode = StatusCodes.Status400BadRequest;

            _brokerApiError = error;
            _message = message;
        }

        public override Task ExecuteResultAsync(ActionContext context) {
            var headers = context.HttpContext.Response.Headers;
            headers.Add(CustomHttpHeaders.RTVSApiError, new StringValues(_brokerApiError.ToString()));
            if (!string.IsNullOrEmpty(_message)) {
                headers.Add(CustomHttpHeaders.RTVSBrokerException, _message);
            }
            return base.ExecuteResultAsync(context);
        }
    }
}
