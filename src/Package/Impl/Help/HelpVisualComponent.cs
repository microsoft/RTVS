// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Idle;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.R.Components.Help;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using mshtml;
using ContentControl = System.Windows.Controls.ContentControl;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class HelpVisualComponent : IHelpVisualComponent {
        /// <summary>
        /// Holds browser control. When R session is restarted
        /// it is necessary to re-create the browser control since
        /// help server changes port and current links stop working.
        /// However, VS tool window doesn't like its child root
        /// control changing so instead we keed content control 
        /// unchanged and only replace browser that is inside it.
        /// </summary>
        private readonly ContentControl _windowContentControl;
        private readonly IVignetteCodeColorBuilder _codeColorBuilder;
        private readonly IServiceContainer _services;
        private readonly IRSession _session;
        private WindowsFormsHost _host;

        public HelpVisualComponent(IServiceContainer services) {
            _services = services;

            _codeColorBuilder = _services.GetService<IVignetteCodeColorBuilder>();
            var workflow = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            workflow.RSessions.BrokerStateChanged += OnBrokerStateChanged;

            _session = workflow.RSession;
            _session.Disconnected += OnRSessionDisconnected;

            _windowContentControl = new ContentControl();

            CreateBrowser();
            VSColorTheme.ThemeChanged += OnColorThemeChanged;
        }

        private void OnColorThemeChanged(ThemeChangedEventArgs e) => SetThemeColors();

        #region IVisualComponent
        public FrameworkElement Control => _windowContentControl;
        public IVisualComponentContainer<IVisualComponent> Container { get; internal set; }

        #endregion

        #region IHelpWindowVisualComponent
        /// <summary>
        /// Browser that displays help content
        /// </summary>
        public WebBrowser Browser { get; private set; }

        public void Navigate(string url) {
            // Filter out localhost help URL from absolute URLs
            // except when the URL is the main landing page.
            var settings = _services.GetService<IRSettings>();
            if (settings.HelpBrowserType == HelpBrowserType.Automatic && IsHelpUrl(url)) {
                Container?.Show(focus: false, immediate: false);
                NavigateTo(url);
            } else {
                _services.Process().Start(url);
            }
        }

        public string VisualTheme { get; set; }
        #endregion

        private void OnRSessionDisconnected(object sender, EventArgs e) {
            // Event fires on a background thread
            _services.MainThread().Post(CloseBrowser);
        }

        private void OnBrokerStateChanged(object sender, BrokerStateChangedEventArgs e) {
            if (!e.IsConnected) {
                // Event mey fire on a background thread
                _services.MainThread().Post(CloseBrowser);
            }
        }

        private void CreateBrowser() {
            if (Browser == null) {
                Browser = new WebBrowser {
                    WebBrowserShortcutsEnabled = true,
                    IsWebBrowserContextMenuEnabled = true
                };

                Browser.Navigating += OnNavigating;
                Browser.Navigated += OnNavigated;

                _host = new WindowsFormsHost();
                _windowContentControl.Content = _host;
            }
        }

        private void SetThemeColors() {
            RemoveExistingStyles();
            AttachStandardStyles();
            AttachCodeStyles();

            // The body may become null after styles are modified.
            // this happens if browser decides to re-render document.
            var doc = Browser?.Document?.DomDocument as IHTMLDocument2;
            if (doc?.body == null) {
                SetThemeColorsWhenReady();
            }
        }

        /// <summary>
        /// Attaches theme-specific styles to the help page.
        /// </summary>
        private void AttachStandardStyles() {
            var doc = Browser?.Document?.DomDocument as IHTMLDocument2;
            if (doc != null) {
                string cssText = GetCssText();
                if (!string.IsNullOrEmpty(cssText)) {
                    IHTMLStyleSheet ss = doc.createStyleSheet();
                    if (ss != null) {
                        ss.cssText = cssText;
                    }
                }
            }
        }

        /// <summary>
        /// Attaches code colorization styles to vignettes.
        /// </summary>
        private void AttachCodeStyles() {
            var doc = Browser?.Document?.DomDocument as IHTMLDocument2;
            if (doc != null) {
                var ss = doc.createStyleSheet();
                if (ss != null) {
                    ss.cssText = _codeColorBuilder.GetCodeColorsCss();
                }
            }
        }

        /// <summary>
        /// Removes existing styles from the help page or vignette.
        /// </summary>
        private void RemoveExistingStyles() {
            var doc = Browser?.Document?.DomDocument as dynamic;
            if (doc != null) {
                if (doc.styleSheets.length > 0) {
                    // Remove stylesheets
                    var styleSheets = new List<dynamic>();
                    foreach (IHTMLStyleSheet s in doc.styleSheets) {
                        s.disabled = true;
                    }
                }
                // Remove style blocks
                foreach (var node in doc.head.childNodes) {
                    if (node is IHTMLStyleElement) {
                        doc.head.removeChild(node);
                    }
                }
            }
        }

        /// <summary>
        /// Fetches theme-specific stylesheet from disk
        /// </summary>
        private string GetCssText() {
            string cssfileName = null;

            if (VisualTheme != null) {
                cssfileName = VisualTheme;
            } else {
                // TODO: We can generate CSS from specific VS colors. For now, just do Dark and Light.
                var ui = _services.UI();
                cssfileName = ui.UIColorTheme == UIColorTheme.Dark ? "Dark.css" : "Light.css";
            }

            if (!string.IsNullOrEmpty(cssfileName)) {
                string assemblyPath = Assembly.GetExecutingAssembly().GetAssemblyPath();
                string themePath = Path.Combine(Path.GetDirectoryName(assemblyPath), @"Help\Themes\", cssfileName);

                try {
                    using (var sr = new StreamReader(themePath)) {
                        return sr.ReadToEnd();
                    }
                } catch (IOException) {
                    Trace.Fail("Unable to load theme stylesheet {0}", cssfileName);
                }
            }
            return string.Empty;
        }

        private void OnNavigating(object sender, WebBrowserNavigatingEventArgs e) {
            // Disconnect browser from the tool window so it does not
            // flicker when we change page and element styling.
            DisconnectBrowser();

            string url = e.Url.ToString();
            if (!IsHelpUrl(url)) {
                e.Cancel = true;
                _services.Process().Start(url);
            }
        }

        private void OnNavigated(object sender, WebBrowserNavigatedEventArgs e) {
            // Page may be loaded, but body may still be null of scripts
            // are running. For example, in 3.2.2 code colorization script
            // tends to damage body content so browser may have to to re-create it.
            SetThemeColorsWhenReady();

            // Upon navigation we need to ask VS to update UI so 
            // Back/Forward buttons become properly enabled or disabled.
            IVsUIShell shell = _services.GetService<IVsUIShell>(typeof(SVsUIShell));
            shell.UpdateCommandUI(0);
        }

        private void OnWindowUnload(object sender, HtmlElementEventArgs e) {
            // Refresh button clicked. Current document state is 'complete'.
            // We need to delay until it changes to 'loading' and then
            // delay again until it changes again to 'complete'.
            DisconnectBrowser();
        }

        private void SetThemeColorsWhenReady() {
            if (!ConnectBrowser()) {
                // The browser document is not ready yet. Create another idle 
                // time action that will run after few milliseconds.
                IdleTimeAction.Create(SetThemeColorsWhenReady, 10, new object(), _services.GetService<IIdleTimeService>());
            }
        }

        private void NavigateTo(string url) {
            if (Browser == null) {
                CreateBrowser();
            }
            Browser.Navigate(url);
        }

        private static bool IsHelpUrl(string url) {
            Uri uri = new Uri(url);
            if(uri.AbsoluteUri.EndsWithIgnoreCase(".pdf")) {
                return false;
            }
            // dynamicHelp.R (startDynamicHelp function):
            // # Choose 10 random port numbers between 10000 and 32000
            // ports <- 10000 + 22000*((stats::runif(10) + unclass(Sys.time())/300) %% 1)
            return uri.IsLoopback && uri.Port >= 10000 && uri.Port <= 32000 && !string.IsNullOrEmpty(uri.PathAndQuery);
        }

        public void Dispose() {
            DisconnectFromSessionEvents();
            CloseBrowser();
            VSColorTheme.ThemeChanged -= OnColorThemeChanged;
        }


        private void DisconnectFromSessionEvents() {
            if (_session != null) {
                _session.Disconnected -= OnRSessionDisconnected;
            }
        }

        private void CloseBrowser() {
            _windowContentControl.Content = null;

            if (Browser != null) {
                DisconnectWindowEvents();

                Browser.Navigating -= OnNavigating;
                Browser.Navigated -= OnNavigated;

                Browser.Dispose();
                Browser = null;
            }
        }

        private bool ConnectBrowser() {
            var doc = Browser?.Document?.DomDocument as IHTMLDocument2;
            if (doc?.body != null && Browser.ReadyState == WebBrowserReadyState.Complete) {
                SetThemeColors();
                Browser.Document.Window.Unload += OnWindowUnload;
                // Reconnect browser control to the window
                _host.Child = Browser;
                return true;
            }
            return false;
        }

        private void DisconnectBrowser() {
            DisconnectWindowEvents();
            // Disconnect browser from the tool window so it does not
            // flicker when we change page and element styling.
            _host.Child = null;
        }

        private void DisconnectWindowEvents() {
            var window = Browser?.Document?.Window;
            if (window != null) {
                window.Unload -= OnWindowUnload;
            }
        }
    }
}
