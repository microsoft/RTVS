// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using static System.FormattableString;

namespace Microsoft.Markdown.Editor.Preview.Code {
    internal static class HtmlFormatter {
        public static string FormatCode(string code, string style = null) 
            => Invariant($"<code style='white-space: pre-wrap; display: block; {style ?? string.Empty}'>{code}</code>");
    }
}
