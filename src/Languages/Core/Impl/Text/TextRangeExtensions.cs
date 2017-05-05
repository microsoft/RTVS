// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Core.Text {
    public static class TextRangeExtensions {
        public static ITextRange Union(this ITextRange range, ITextRange other) => TextRange.Union(range, other);
        public static bool Intersect(this ITextRange range, ITextRange other) => TextRange.Intersect(range, other);
    }
}
