// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Text;
using Microsoft.Html.Core.Parser;

namespace Microsoft.R.Support.RD.Parser {
    internal class PlainTextExtractor {
        private readonly List<int> _tagPositions = new List<int>();

        public string GetTextFromHtml(string html) {
            var parser = new HtmlParser();

            parser.StartTagOpen += OnStartTagOpen;
            parser.StartTagClose += OnStartTagClose;
            parser.EndTagOpen += OnEndTagOpen;
            parser.EndTagClose += OnEndTagClose;

            parser.Parse(html);

            if (_tagPositions.Count > 0) {
                var sb = new StringBuilder(html);
                int offset = 0;
                for (int i = 0; i < _tagPositions.Count; i += 2) {
                    // Calculate current tag positions
                    int start = _tagPositions[i] + offset;
                    int end = (i < _tagPositions.Count - 1 ? _tagPositions[i + 1] : sb.Length) + offset;
                    int length = end - start;
                    // Remove HTML tag
                    sb.Remove(start, length);
                    // Account for shortening of the string relatively to tag positions in the original string
                    offset -= length;
                }
                return sb.ToString();
            }
            return html;
        }

        private void OnStartTagOpen(object sender, HtmlParserOpenTagEventArgs e) {
            _tagPositions.Add(e.OpenAngleBracketPosition);
        }

        private void OnStartTagClose(object sender, HtmlParserCloseTagEventArgs e) {
            _tagPositions.Add(e.CloseAngleBracket.End);
        }

        private void OnEndTagOpen(object sender, HtmlParserOpenTagEventArgs e) {
            _tagPositions.Add(e.OpenAngleBracketPosition);
        }
        private void OnEndTagClose(object sender, HtmlParserCloseTagEventArgs e) {
            _tagPositions.Add(e.CloseAngleBracket.End);
        }
    }
}
