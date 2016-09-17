// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker {
    internal sealed class ApiErrorResult : ObjectResult {
        public ApiErrorResult(HttpResponse response, BrokerApiError error) : base(error) {
            StatusCode = StatusCodes.Status412PreconditionFailed;
            response.Headers.Add(CustomHttpHeaders.RTVSApiError, new Extensions.Primitives.StringValues(error.ToString()));
        }
    }
}
