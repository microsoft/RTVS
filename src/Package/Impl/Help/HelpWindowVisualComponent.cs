using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Threading;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using mshtml;
using ContentControl = System.Windows.Controls.ContentControl;

namespace Microsoft.VisualStudio.R.Package.Help {
    public sealed class HelpWindowVisualComponent : IHelpWindowVisualComponent {
        /// <summary>
        /// Holds browser control. When R session is restarted
        /// it is necessary to re-create the browser control since
        /// help server changes port and current links stop working.
        /// However, VS tool window doesn't like its child root
        /// control changing so instead we keed content control 
        /// unchanged and only replace browser that is inside it.
        /// </summary>
        private ContentControl _windowContentControl;
        private IRSession _session;
        private WindowsFormsHost _host;

        public HelpWindowVisualComponent() {
            _session = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>().GetInteractiveWindowRSession();
            _session.Disconnected += OnRSessionDisconnected;

            _windowContentControl = new System.Windows.Controls.ContentControl();
            this.Control = _windowContentControl;

            var c = new Controller();
            c.AddCommandSet(GetCommands());
            this.Controller = c;

            CreateBrowser();
            VSColorTheme.ThemeChanged += OnColorThemeChanged;
        }

        private void OnColorThemeChanged(ThemeChangedEventArgs e) {
            SetThemeColors();
        }

        #region IVisualComponent
        public ICommandTarget Controller { get; }

        public System.Windows.Controls.Control Control { get; }
        #endregion

        #region IHelpWindowVisualComponent
        /// <summary>
        /// Browser that displays help content
        /// </summary>
        public WebBrowser Browser { get; private set; }

        public void Navigate(string url) {
            // Filter out localhost help URL from absolute URLs
            // except when the URL is the main landing page.
            if (RToolsSettings.Current.HelpBrowser == HelpBrowserType.Automatic && IsHelpUrl(url)) {
                NavigateTo(url);
            } else {
                Process.Start(url);
            }
        }

        public string VisualTheme { get; set; }
        #endregion

        private void OnRSessionDisconnected(object sender, EventArgs e) {
            // Event fires on a background thread
            VsAppShell.Current.DispatchOnUIThread(() => {
                CloseBrowser();
            });
        }

        private void CreateBrowser() {
            if (Browser == null) {
                Browser = new WebBrowser();

                Browser.WebBrowserShortcutsEnabled = true;
                Browser.IsWebBrowserContextMenuEnabled = true;

                Browser.Navigating += OnNavigating;
                Browser.Navigated += OnNavigated;

                _host = new WindowsFormsHost();
                _windowContentControl.Content = _host;
            }
        }

        private void SetThemeColors() {
            if (Browser != null) {
                string cssText = GetCssText();
                if (!string.IsNullOrEmpty(cssText) && Browser.Document != null) {
                    IHTMLDocument2 doc = Browser.Document.DomDocument as IHTMLDocument2;
                    if (doc != null) {
                        if (doc.styleSheets.length > 0) {
                            object index = 0;
                            var ss = doc.styleSheets.item(ref index) as IHTMLStyleSheet;
                            ss.cssText = cssText;
                        } else {
                            IHTMLStyleSheet ss = doc.createStyleSheet();
                            if (ss != null) {
                                ss.cssText = cssText;
                            }
                        }
                    }
                }
            }
        }

        private string GetCssText() {
            string cssfileName = null;

            if (VisualTheme != null) {
                cssfileName = VisualTheme;
            } else {
                Color defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
                // TODO: We can generate CSS from specific VS colors. For now, just do Dark and Light.
                cssfileName = defaultBackground.GetBrightness() < 0.5 ? "Dark.css" : "Light.css";
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
            string url = e.Url.ToString();
            if (!IsHelpUrl(url)) {
                e.Cancel = true;
                Process.Start(url);
            }
        }

        private void OnNavigated(object sender, WebBrowserNavigatedEventArgs e) {
            SetThemeColors();
            _host.Child = Browser;

            // Upon vavigation we need to ask VS to update UI so 
            // Back /Forward buttons become properly enabled or disabled.
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            shell.UpdateCommandUI(0);
        }

        private void NavigateTo(string url) {
            if (Browser == null) {
                CreateBrowser();
            }
            Browser.Navigate(url);
        }

        private static bool IsHelpUrl(string url) {
            Uri uri = new Uri(url);
            // dynamicHelp.R (startDynamicHelp function):
            // # Choose 10 random port numbers between 10000 and 32000
            // ports <- 10000 + 22000*((stats::runif(10) + unclass(Sys.time())/300) %% 1)
            return uri.IsLoopback && uri.Port >= 10000 && uri.Port <= 32000 && !string.IsNullOrEmpty(uri.PathAndQuery);
        }

        private IEnumerable<ICommand> GetCommands() {
            List<ICommand> commands = new List<ICommand>() {
                new HelpPreviousCommand(this),
                new HelpNextCommand(this),
                new HelpHomeCommand(this),
                new HelpRefreshCommand(this)
            };
            return commands;
        }

        public void Dispose() {
            DisconnectFromSessionEvents();
            CloseBrowser();
            VSColorTheme.ThemeChanged -= OnColorThemeChanged;
        }


        private void DisconnectFromSessionEvents() {
            if (_session != null) {
                _session.Disconnected -= OnRSessionDisconnected;
                _session = null;
            }
        }

        private void CloseBrowser() {
            _windowContentControl.Content = null;

            if (Browser != null) {
                Browser.Navigating -= OnNavigating;
                Browser.Navigated -= OnNavigated;
                Browser.Dispose();
                Browser = null;
            }
        }
    }
}
