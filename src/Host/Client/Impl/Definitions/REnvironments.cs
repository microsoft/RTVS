// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Expressions for various common R environments, suitable for use as <c>environmentExpression</c> argument
    /// when calling <see cref="RSessionExtensions.EvaluateAndDescribeAsync"/>.
    /// </summary>
    public static class REnvironments {
        public static readonly string GlobalEnv = "base::.GlobalEnv";
        public static readonly string BaseEnv = "base::baseenv()";
    }
}
