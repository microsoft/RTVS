// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Markdown.Editor.Utility {
    public static class MarkdownUtility {
        /// <summary>
        /// Converts R code block as it appears in R Markdown to legal R.
        /// Changes decoration like '{r, x = FALSE, ...} into
        /// x = FALSE; y = 1.0;  Allows for brace nesting.
        /// </summary>
        /// <param name="content"></param>
        /// <remarks>
        /// http://rmarkdown.rstudio.com/developer_parameterized_reports.html#accessing_from_r
        /// </remarks>
        public static string GetRContentFromMarkdownCodeBlock(string content) {
            while (true) {
                // Locate start of the block
                var start = content.IndexOfIgnoreCase("{r");
                if (start < 0) {
                    break;
                }
                // Locate the closing curly brace
                var bc = new BraceCounter<char>('{', '}');
                var end = start;
                bc.CountBrace(content[end]);
                while (bc.Count > 0 && end < content.Length) {
                    end++;
                    bc.CountBrace(content[end]);
                }
                // Remove {r and the closing }
                if (end >= content.Length || end <= start) {
                    break;
                }
                content = content.Remove(end, 1);
                content = content.Remove(start, 2);
                content = content.Replace(",", ";", start, end - start);
            }
            // Remove parameter lines like params$x as well as leading ' from lines if any
            var lines = content.Split(CharExtensions.LineBreakChars, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            for(int i = 0; i < lines.Length; i++) { 
                var index = lines[i].IndexOf(';');
                if (index >= 0  && string.IsNullOrWhiteSpace(lines[i].Substring(0, index))) {
                    sb.Append(lines[i].Substring(index + 1));
                } else {
                    index = lines[i].IndexOfOrdinal("params$");
                    if(index < 0) {
                        sb.Append(lines[i]);
                    }
                }
                sb.Append(Environment.NewLine);
            }
            content = sb.ToString();
            return content;
        }
    }
}
