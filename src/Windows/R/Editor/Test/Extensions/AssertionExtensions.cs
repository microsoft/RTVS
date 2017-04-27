// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Test.Assertions;

namespace Microsoft.R.Editor.Test {
    [ExcludeFromCodeCoverage]
    internal static class AssertionExtensions {
        public static FunctionSignatureAssertion Should(this RFunctionSignatureInfo si) {
            return new FunctionSignatureAssertion(si);
        }
    }
}