// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Markdown.Editor.Utility {
    public static class MarkdownUtility {
        /// <summary>
        /// Converts R code block as it appears in R Markdown to legal R.
        /// Drops options setting block '{r, x = FALSE, ...}.
        /// Allows for curly brace nesting.
        /// </summary>
        /// <param name="content"></param>
        /// <remarks>
        /// http://rmarkdown.rstudio.com/developer_parameterized_reports.html#accessing_from_r
        /// </remarks>
        public static string GetRContentFromMarkdownCodeBlock(string content) {
            // Locate start of the block
            var start = content.IndexOfIgnoreCase("{r");
            if (start >= 0) {
                // Locate the closing curly brace
                var bc = new BraceCounter<char>('{', '}');
                var end = start;
                bc.CountBrace(content[end]);
                while (bc.Count > 0 && end < content.Length) {
                    end++;
                    bc.CountBrace(content[end]);
                }
                // Remove {r ... }
                if (end < content.Length && end > start) {
                    content = content.Remove(start, end - start + 1);
                    // Remove parameter lines like params$x as well
                    var lines = content.Split(CharExtensions.LineBreakChars, StringSplitOptions.RemoveEmptyEntries);
                    var sb = new StringBuilder();
                    for (int i = 0; i < lines.Length; i++) {
                        var index = lines[i].IndexOfOrdinal("params$");
                        if (index < 0) {
                            sb.Append(lines[i]);
                            sb.Append(Environment.NewLine);
                        }
                    }
                    content = sb.ToString();
                }
            }
            return content;
        }
    }
}
