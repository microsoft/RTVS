// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using mshtml;

namespace Microsoft.Markdown.Editor.Preview.Browser {
    internal sealed class ScrollTracker {
        private const string _linePragmaPrefix = "pragma-line-";
        private readonly HTMLDocument _htmlDocument;
        private int _firstVisibleLine = -1;
        private int _viewTop = -1;
        private int _viewBottom = -1;

        public ScrollTracker(HTMLDocument htmlDocument) {
            _htmlDocument = htmlDocument;
        }

        /// <summary>
        /// Given line in the markdown document attempts to locate matching
        /// element in the browser DOM and bring it to the view.
        /// </summary>
        /// <param name="markdownLineNumber"></param>
        public void SetScrollPosition(int markdownLineNumber) {
            if (markdownLineNumber <= 0) {
                // Forces the preview window to scroll to the top of the document
                _htmlDocument.documentElement.setAttribute("scrollTop", 0);
            } else {
                var element = _htmlDocument.getElementById(_linePragmaPrefix + markdownLineNumber);
                element?.scrollIntoView(true);
            }
            Invalidate();
        }

        public int GetFirstVisibleLineNumber() {
            if (_htmlDocument?.documentElement == null) {
                return -1;
            }

            var de = _htmlDocument.documentElement;
            var de2 = _htmlDocument.documentElement as IHTMLElement2;
            var viewTop = de2.scrollTop;
            var viewBottom = viewTop + de.offsetHeight;

            if (_viewTop == viewTop && _viewBottom == viewBottom && _firstVisibleLine >= 0) {
                return _firstVisibleLine;
            }

            _viewTop = viewTop;
            _viewBottom = viewBottom;

            _firstVisibleLine = FindFirstVisibleParagraphLine();
            return _firstVisibleLine;
        }

        public void Invalidate() => _firstVisibleLine = -1;

        private int FindFirstVisibleParagraphLine() {
            var allPara = _htmlDocument.getElementsByTagName("p");
            if (allPara == null) {
                return -1;
            }

            for (var i = 0; i < allPara.length; i++) {
                var e = allPara.item(i) as IHTMLElement;
                if (e == null || e.offsetTop >= _viewBottom) {
                    break;
                }
                var center = e.offsetTop + e.offsetHeight / 2;
                if (center >= _viewTop) {
                    var id = e.id;
                    if (id != null && id.StartsWithOrdinal(_linePragmaPrefix)) {
                        return int.TryParse(id.Substring(_linePragmaPrefix.Length), out int value) ? value : -1;
                    }
                }
            }

            return -1;
        }
    }
}
