// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.AspNetCore.Server.Kestrel.Internal.Networking;

namespace Microsoft.R.Host.Broker {
    public static class ExceptionExtensions {
        public static bool IsPortInUseException(this AggregateException aggex) {
            return
                aggex.InnerExceptions.Count == 1 &&
                aggex.InnerException is UvException &&
                ((UvException)aggex.InnerException).StatusCode == -4091; // Error Address in Use
        }
    }
}
