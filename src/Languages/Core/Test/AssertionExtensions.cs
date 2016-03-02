// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Assertions;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.Languages.Core.Test {
    [ExcludeFromCodeCoverage]
    internal static class AssertionExtensions {
        public static TokenAssertions<TTokenType> Should<TTokenType>(this IToken<TTokenType> token) {
            return new TokenAssertions<TTokenType>(token);
        }
    }
}