// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core {
    public static class RandomExtensions {
        public static int GetEphemeralPort(this Random random) {
            return random.Next(49152, 65535);
        }
    }
}
