// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Drawing;
using Microsoft.Common.Core;
using mshtml;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Markdown.Editor.Preview.Browser {
    internal sealed class ScrollTracker : IDisposable {
        private const string _linePragmaPrefix = "pragma-line-";

        private readonly HTMLDocument _htmlDocument;
        private readonly DisposableBag _disposableBag = new DisposableBag(nameof(ScrollTracker));

        private int _firstVisibleLine = -1;
        private Rectangle _viewRect;
        private bool _mouseOver;

        public event EventHandler<EventArgs> ViewportChange;

        public ScrollTracker(HTMLDocument htmlDocument, IIdleTimeService idleTimeService) {
            _htmlDocument = htmlDocument;

            var documentEvents = (HTMLDocumentEvents2_Event)_htmlDocument;

            // Unfortunately, window.onscroll and body.onscroll do not fire in C#.
            // Therefore we will be tracking if scroll was initiated in the browser
            // pane by tracking mouse in/out and mouse wheel.
            // If mouse is over the window and viewport position changed
            // then we consider it to be a scroll event.
            documentEvents.onmousewheel += OnMouseWheel;
            documentEvents.onmouseover += OnMouseOver;
            documentEvents.onmouseout += OnMouseOut;

            idleTimeService.Idle += OnIdle;

            _disposableBag
                .Add(() => documentEvents.onmousewheel -= OnMouseWheel)
                .Add(() => documentEvents.onmouseover -= OnMouseOver)
                .Add(() => documentEvents.onmouseout -= OnMouseOut)
                .Add(() => idleTimeService.Idle -= OnIdle);
        }

        private void OnMouseOver(IHTMLEventObj e) =>_mouseOver = true;
        private void OnMouseOut(IHTMLEventObj e)  => _mouseOver = _htmlDocument.elementFromPoint(e.x, e.y) != null;

        private bool OnMouseWheel(IHTMLEventObj pEvtObj) {
            Invalidate();
            ViewportChange?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private void OnIdle(object sender, EventArgs e) {
            if (_mouseOver) {
                var rc = GetViewRect();
                if (_viewRect.Left != rc.Left || _viewRect.Top != rc.Top || _viewRect.Width != rc.Width || _viewRect.Height != rc.Height) {
                    Invalidate();
                    ViewportChange?.Invoke(this, EventArgs.Empty);
                }
            }
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
                if (element != null) {
                    _htmlDocument.parentWindow.scroll(0, element.offsetTop - _htmlDocument.documentElement.offsetTop);
                    //element.scrollIntoView(true);
                }
            }
            Invalidate();
        }

        public int GetFirstVisibleLineNumber() {
            if (_htmlDocument?.documentElement == null) {
                return -1;
            }

            var rc = GetViewRect();

            if (_viewRect.Top == rc.Top && _viewRect.Bottom == rc.Bottom && _firstVisibleLine >= 0) {
                return _firstVisibleLine;
            }

            _viewRect = GetViewRect();
            _firstVisibleLine = FindFirstVisibleParagraphLine();
            return _firstVisibleLine;
        }

        public void Invalidate() {
            _firstVisibleLine = -1;
            _viewRect = GetViewRect();
        }

        private bool IsVisible(IHTMLElement e) => GetViewRect().Contains(e.offsetLeft, e.offsetTop);

        private Rectangle GetViewRect() {
            var de2 = (IHTMLElement2)_htmlDocument.documentElement;
            return new Rectangle(de2.scrollLeft, de2.scrollTop, de2.clientWidth, de2.clientHeight);
        }

        private int FindFirstVisibleParagraphLine() {
            var allPara = _htmlDocument.getElementsByTagName("p");
            if (allPara == null) {
                return -1;
            }

            for (var i = 0; i < allPara.length; i++) {
                var e = allPara.item(i) as IHTMLElement;
                if (e == null || e.offsetTop >= _viewRect.Bottom) {
                    break;
                }
                var center = e.offsetTop + e.offsetHeight / 2;
                if (center >= _viewRect.Top) {
                    var id = e.id;
                    if (id != null && id.StartsWithOrdinal(_linePragmaPrefix)) {
                        return int.TryParse(id.Substring(_linePragmaPrefix.Length), out int value) ? value : -1;
                    }
                }
            }

            return -1;
        }

        public void Dispose() => _disposableBag.TryDispose();
    }
}
