// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core {
    public static class LongExtensions {
        public static int ReduceToInt(this long value) => value > int.MaxValue ? int.MaxValue : value < int.MinValue ? int.MinValue : (int) value;
    }
}