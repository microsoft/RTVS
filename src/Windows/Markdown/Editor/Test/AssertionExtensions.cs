// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Assertions;
using Microsoft.Markdown.Editor.Tokens;

namespace Microsoft.Markdown.Editor.Test {
    [ExcludeFromCodeCoverage]
    internal static class AssertionExtensions {
        public static TokenAssertions<MarkdownTokenType> Should(this MarkdownToken token) {
            return new TokenAssertions<MarkdownTokenType>(token);
        }
    }
}