// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Shell {
    public static class DateTimeExtensions {
        public static int MillisecondsSinceUtc(this DateTime since) {
            var diff = DateTime.UtcNow - since;
            return (int)diff.TotalMilliseconds;
        }
    }
}
