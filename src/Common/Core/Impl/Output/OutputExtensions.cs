// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Common.Core.Output {
    public static class OutputExtensions {
        public static void WriteLine(this IOutput output, string value) => output.Write(value + Environment.NewLine);
    }
}