// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.Common.Core {
    public static class EnumExtensions {
        public static bool IsAny(this Enum @enum, params Enum[] values) => values.Contains(@enum);
    }
}