// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using Markdig.Syntax;

namespace Microsoft.Markdown.Editor.Preview.Parser {
    internal static class Extensions {
        public static string GetText(this LeafBlock block) {
            if(block?.Lines.Lines == null) {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var line in block.Lines.Lines) {
                sb.AppendLine(line.ToString());
            }
            return sb.ToString().Trim();
        }
    }
}
