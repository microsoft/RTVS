// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Core.Utility {
    public static class TimeUtility {
        public static int MillisecondsSinceUtc(DateTime since) {
            var diff = DateTime.UtcNow - since;
            return (int)diff.TotalMilliseconds;
        }
    }
}
