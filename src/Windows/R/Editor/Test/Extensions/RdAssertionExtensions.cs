// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Assertions;
using Microsoft.R.Editor.RData.Tokens;

namespace Microsoft.R.Editor.Test {
    [ExcludeFromCodeCoverage]
    internal static class RdAssertionExtensions {
        public static TokenAssertions<RdTokenType> Should(this RdToken token) {
            return new TokenAssertions<RdTokenType>(token);
        }
    }
}