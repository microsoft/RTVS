// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker {
    internal sealed class CustomErrorResult : ObjectResult {
        public static IActionResult Create(CustomHttpError error) {
            return new CustomErrorResult((int)error) as IActionResult;
        }
        private CustomErrorResult(int code) : base(code) {
            StatusCode = code;
        }
    }
}
