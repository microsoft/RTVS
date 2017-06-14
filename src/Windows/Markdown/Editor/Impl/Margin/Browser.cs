// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Markdig.Syntax;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Editor;
using Microsoft.VisualStudio.Text;
using mshtml;
using Microsoft.Common.Core.Extensions;
using static System.FormattableString;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace Microsoft.Markdown.Editor.Margin {
    // Based on https://github.com/madskristensen/MarkdownEditor/blob/master/src/Margin/Browser.cs
    public sealed class Browser : IDisposable {
        private readonly IServiceContainer _services;
        private readonly RMarkdownOptions _options;
        private readonly string _fileName;
        private readonly int _zoomFactor;
        private readonly DocumentRenderer _documentRenderer;

        private HTMLDocument _htmlDocument;
        private double _cachedPosition;
        private double _cachedHeight;
        private double _positionPercentage;

        private MarkdownDocument _currentDocument;
        private int _currentViewLine = -1;

        public Browser(string fileName, IServiceContainer services) {
            Check.ArgumentNull(nameof(fileName), fileName);
            Check.ArgumentNull(nameof(services), services);

            _fileName = fileName;
            _services = services;
            _options = _services.GetService<IREditorSettings>().MarkdownOptions;

            _zoomFactor = GetZoomFactor();
            InitBrowser();

            _documentRenderer = new DocumentRenderer(Path.GetFileName(_fileName), _services);

            CssCreationListener.StylesheetUpdated += OnStylesheetUpdated;
        }

        public WebBrowser Control { get; private set; }

        private void InitBrowser() {
            Control = new WebBrowser { HorizontalAlignment = HorizontalAlignment.Stretch };

            Control.LoadCompleted += (s, e) => {
                Zoom(_zoomFactor);
                _htmlDocument = (HTMLDocument)Control.Document;

                _cachedHeight = _htmlDocument.body.offsetHeight;
                _htmlDocument.documentElement.setAttribute("scrollTop", _positionPercentage * _cachedHeight / 100);
                AdjustAnchors();
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

            // If it's a file-based anchor we converted, open the related file if possible
            if (e.Uri.Scheme.EqualsOrdinal("about")) {
                var file = e.Uri.LocalPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

                if (file.EqualsOrdinal("blank")) {
                    var fragment = e.Uri.Fragment?.TrimStart('#');
                    NavigateToFragment(fragment);
                    return;
                }

                var fs = _services.GetService<IFileSystem>();
                if (!fs.FileExists(file)) {
                    string ext = null;

                    // If the file has no extension, see if one exists with a markdown extension.
                    // If so, treat it as the file to open.
                    if (string.IsNullOrEmpty(Path.GetExtension(file))) {
                        ext = MdContentTypeDefinition.FileExtension;
                    }

                    if (ext != null) {
                        //ProjectHelpers.OpenFileInPreviewTab(Path.ChangeExtension(file, ext));
                    }
                } else {
                    //ProjectHelpers.OpenFileInPreviewTab(file);
                }
            } else if (e.Uri.IsAbsoluteUri && e.Uri.Scheme.StartsWith("http")) {
                var ps = _services.GetService<IProcessServices>();
                ps.Start(e.Uri.ToString());
            }
        }


        private void NavigateToFragment(string fragmentId)
            => _htmlDocument.getElementById(fragmentId)?.scrollIntoView(true);

        /// <summary>
        /// Adjust the file-based anchors so that they are navigable on the local file system
        /// </summary>
        /// <remarks>Anchors using the "file:" protocol appear to be blocked by security settings and won't work.
        /// If we convert them to use the "about:" protocol so that we recognize them, we can open the file in
        /// the <c>Navigating</c> event handler.</remarks>
        private void AdjustAnchors() {
            try {
                foreach (var link in _htmlDocument.links) {
                    var anchor = link as HTMLAnchorElement;

                    if (anchor != null && anchor.protocol.EqualsOrdinal("file:")) {
                        string pathName = null, hash = anchor.hash;

                        // Anchors with a hash cause a crash if you try to set the protocol without clearing the
                        // hash and path name first.
                        if (hash != null) {
                            pathName = anchor.pathname;
                            anchor.hash = null;
                            anchor.pathname = string.Empty;
                        }

                        anchor.protocol = "about:";

                        if (hash != null) {
                            // For an in-page section link, use "blank" as the path name.  These don't work
                            // anyway but this is the proper way to handle them.
                            if (pathName == null || pathName.EndsWith("/")) {
                                pathName = "blank";
                            }
                            anchor.pathname = pathName;
                            anchor.hash = hash;
                        }
                    }
                }
            } catch(Exception ex) when (!ex.IsCriticalException()) { }
        }

        private static int GetZoomFactor() {
            using (var g = Graphics.FromHwnd(Process.GetCurrentProcess().MainWindowHandle)) {
                const int baseLine = 96;
                var dpi = g.DpiX;

                if (baseLine == (int)dpi) {
                    return 100;
                }

                // 150% scaling => 225
                // 250% scaling => 400

                double scale = dpi * ((dpi - baseLine) / baseLine + 1);
                return Convert.ToInt32(Math.Ceiling(scale / 25)) * 25; // round up to nearest 25
            }
        }

        public void UpdatePosition(int line) {
            if (_htmlDocument != null && _currentDocument != null && _options.AutomaticSync) {
                _currentViewLine = _currentDocument.FindClosestLine(line);
                SyncNavigation();
            }
        }

        private void SyncNavigation() {
            if (_htmlDocument == null) {
                return;
            }

            if (_options.AutomaticSync) {
                if (_currentViewLine == 0) {
                    // Forces the preview window to scroll to the top of the document
                    _htmlDocument.documentElement.setAttribute("scrollTop", 0);
                } else {
                    var element = _htmlDocument.getElementById("pragma-line-" + _currentViewLine);
                    element?.scrollIntoView(true);
                }
            } else if (_htmlDocument != null) {
                _currentViewLine = -1;
                _cachedPosition = _htmlDocument.documentElement.getAttribute("scrollTop");
                _cachedHeight = Math.Max(1.0, _htmlDocument.body.offsetHeight);
                _positionPercentage = _cachedPosition * 100 / _cachedHeight;
            }
        }

        public Task UpdateBrowserAsync(ITextSnapshot snapshot) => Task.Run(() => UpdateBrowser(snapshot));

        private void UpdateBrowser(ITextSnapshot snapshot) {
            // Generate the HTML document
            string html;
            try {
                _currentDocument = snapshot.ParseToMarkdown();
                html = _documentRenderer.RenderStaticHtml(_currentDocument);
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
                    AdjustAnchors();
                } else {
                    html = GetHtmlTemplate().FormatInvariant(html);
                    Control.NavigateToString(html);
                }
                if (_htmlDocument != null) {
                    _documentRenderer.RenderCodeBlocks(_htmlDocument);
                }
                SyncNavigation();
            });
        }

        private void OnStylesheetUpdated(object sender, EventArgs e) {
            var link = _htmlDocument?.styleSheets?.item(0) as IHTMLStyleSheet;
            if (link != null) {
                link.href = GetCustomStylesheet(_fileName) + "?" + new Guid();
            }
        }

        private static string GetCustomStylesheet(string markdownFile)
            => Path.ChangeExtension(markdownFile, "css");

        private string GetHtmlTemplate()
            => $@"<!DOCTYPE html>
<html lang='en'>
    <head>
        <meta http-equiv='X-UA-Compatible' content='IE=Edge' />
        <meta charset='utf-8' />
        <title>Markdown Preview</title>
</head>
    <body class='markdown-body'>
        <div id='___markdown-content___'>
          {{0}}
        </div>
    </body>
</html>";

        private void Zoom(int zoomFactor) {
            if (zoomFactor == 100)
                return;

            dynamic OLECMDEXECOPT_DODEFAULT = 0;
            dynamic OLECMDID_OPTICAL_ZOOM = 63;
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);

            var objComWebBrowser = fiComWebBrowser?.GetValue(Control);
            objComWebBrowser?.GetType().InvokeMember("ExecWB", BindingFlags.InvokeMethod, null, objComWebBrowser, new object[] {
                OLECMDID_OPTICAL_ZOOM,
                OLECMDEXECOPT_DODEFAULT,
                zoomFactor,
                IntPtr.Zero
            });
        }

        public void Dispose() {
            Control?.Dispose();
            _documentRenderer?.Dispose();
            _htmlDocument = null;
        }
    }
}