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
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.Preview.Css;
using Microsoft.Markdown.Editor.Preview.Parser;
using Microsoft.VisualStudio.Text;
using mshtml;
using static System.FormattableString;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace Microsoft.Markdown.Editor.Preview.Browser {
    // Based on https://github.com/madskristensen/MarkdownEditor/blob/master/src/Margin/Browser.cs
    public sealed class BrowserView : IDisposable {
        private static string _htmlTemplate;

        private readonly IServiceContainer _services;
        private readonly int _zoomFactor;
        private readonly DocumentRenderer _documentRenderer;
        private WebBrowserHostUIHandler _uiHandler;

        private HTMLDocument _htmlDocument;
        private MarkdownDocument _currentDocument;

        public BrowserView(string fileName, IServiceContainer services) {
            Check.ArgumentNull(nameof(fileName), fileName);
            Check.ArgumentNull(nameof(services), services);

            _services = services;

            _zoomFactor = GetZoomFactor();
            InitBrowser();

            _documentRenderer = new DocumentRenderer(Path.GetFileName(fileName), _services);
            //CssCreationListener.StylesheetUpdated += OnStylesheetUpdated;
        }

        public WebBrowser Control { get; private set; }

        private void InitBrowser() {
            Control = new WebBrowser { HorizontalAlignment = HorizontalAlignment.Stretch };
            _uiHandler = new WebBrowserHostUIHandler(Control) { IsWebBrowserContextMenuEnabled = false };

            Control.LoadCompleted += (s, e) => {
                Zoom(_zoomFactor);
                _htmlDocument = (HTMLDocument)Control.Document;
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
                var ps = _services.GetService<IProcessServices>();
                ps.Start(e.Uri.ToString());
            }
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
                } else {
                    html = GetPageHtml(html);
                    Control.NavigateToString(html);
                }
                if (_htmlDocument != null) {
                    _documentRenderer.RenderCodeBlocks(_htmlDocument);
                }
            });
        }

        //private void OnStylesheetUpdated(object sender, EventArgs e) {
        //    var link = _htmlDocument?.styleSheets?.item(0) as IHTMLStyleSheet;
        //    if (link != null) {
        //        link.href = GetCustomStylesheet(_fileName) + "?" + new Guid();
        //    }
        //}

        //private static string GetCustomStylesheet(string markdownFile)
        //    => Path.ChangeExtension(markdownFile, "css");

        private string GetPageHtml(string body) {
            if (_htmlTemplate == null) {
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetAssemblyPath());
                var path = Path.Combine(dir, "Markdown", "PreviewTemplate.html");
                _htmlTemplate = _services.FileSystem().ReadAllText(path);
            }
            return _htmlTemplate.Replace("_BODY_", body);
        }

        private void Zoom(int zoomFactor) {
            if (zoomFactor == 100) {
                return;
            }

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