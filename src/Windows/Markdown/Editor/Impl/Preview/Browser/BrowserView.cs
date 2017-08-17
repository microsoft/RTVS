// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Markdig.Syntax;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.Preview.Parser;
using Microsoft.VisualStudio.Text;
using mshtml;
using static System.FormattableString;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace Microsoft.Markdown.Editor.Preview.Browser {
    // Based on https://github.com/madskristensen/MarkdownEditor/blob/master/src/Margin/Browser.cs
    public sealed class BrowserView : IDisposable {
        private readonly IServiceContainer _services;
        private readonly DocumentRenderer _documentRenderer;
        private readonly BrowserWindow _browserWindow;

        private HTMLDocument _htmlDocument;
        private MarkdownDocument _markdownDocument;
        private ScrollTracker _scrollTracker;
        private int _currentMarkdownLineNumber = -1;

        public BrowserView(string fileName, IServiceContainer services) {
            Check.ArgumentNull(nameof(fileName), fileName);
            Check.ArgumentNull(nameof(services), services);

            _services = services;

            InitBrowser();
            _browserWindow = new BrowserWindow(Control);
            _documentRenderer = new DocumentRenderer(Path.GetFileName(fileName), _services);
        }

        public WebBrowser Control { get; private set; }

        private void InitBrowser() {
            Control = new WebBrowser { HorizontalAlignment = HorizontalAlignment.Stretch };
            Control.LoadCompleted += (s, e) => {
                _browserWindow.Init();;
                _htmlDocument = (HTMLDocument)Control.Document;
                _scrollTracker = new ScrollTracker(_htmlDocument);
                _documentRenderer.RenderCodeBlocks(_htmlDocument);
            };

            // Open external links in default browser
            Control.Navigating += OnNavigating;
        }

        private void OnNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e) {
            if (e.Uri == null) {
                return;
            }

            e.Cancel = true;
            if (e.Uri.IsAbsoluteUri && e.Uri.Scheme.StartsWith("http")) {
                _services.Process().Start(e.Uri.ToString());
            }
        }


        public async Task UpdateBrowserAsync(ITextSnapshot snapshot) {
            await TaskUtilities.SwitchToBackgroundThread();
            UpdateBrowser(snapshot);
        }

        public Task UpdateBlocksAsync(ITextSnapshot snapshot, int start, int count)
            => _documentRenderer.RenderCodeBlocks(_htmlDocument, start, count);

        public void UpdateBrowser(ITextSnapshot snapshot) {
            // Generate the HTML document
            string html;
            try {
                _markdownDocument = snapshot.ParseToMarkdown();
                html = _documentRenderer.RenderStaticHtml(_markdownDocument, snapshot.GetText());
            } catch (Exception ex) {
                // We could output this to the exception pane of VS?
                // Though, it's easier to output it directly to the browser
                var message = ex.ToString().Replace("<", "&lt;").Replace("&", "&amp;");
                html = Invariant($"<p>{Resources.BrowserView_Error}:</p><pre>{message}</pre>");
            }

            _services.MainThread().Post(() => {
                var content = _htmlDocument?.getElementById("___markdown-content___");
                // Content may be null if the Refresh context menu option is used.  If so, reload the template.
                if (content != null) {
                    content.innerHTML = html;
                    // Adjust the anchors after and edit
                } else {
                    html = HtmlPageTemplate.GetPageHtml(_services.FileSystem(), html);
                    Control.NavigateToString(html);
                }
                if (_htmlDocument != null) {
                    _documentRenderer.RenderCodeBlocks(_htmlDocument);
                    _scrollTracker.Invalidate();
                    _scrollTracker.SetScrollPosition(_currentMarkdownLineNumber);
                }
            });
        }

        public void UpdatePosition(int textLineNumber) {
            if (_htmlDocument != null && _markdownDocument != null) {
                _currentMarkdownLineNumber = _markdownDocument.FindClosestLine(textLineNumber);
                _scrollTracker.SetScrollPosition(_currentMarkdownLineNumber);
            }
        }

        public void Reload(ITextSnapshot snapshot) {
            _documentRenderer.Reset();
            UpdateBrowser(snapshot);
        }

        public int GetFirstVisibleLineNumber() => _scrollTracker?.GetFirstVisibleLineNumber() ?? 0;

        public void Dispose() {
            Control?.Dispose();
            _documentRenderer?.Dispose();
            _htmlDocument = null;
        }
    }
}