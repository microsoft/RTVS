// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Assertions;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.Test {
    [ExcludeFromCodeCoverage]
    internal static class AssertionExtensions {
        public static TokenAssertions<RdTokenType> Should(this RdToken token) {
            return new TokenAssertions<RdTokenType>(token);
        }
    }
}