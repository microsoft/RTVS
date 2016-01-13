using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Help {
    [Guid(WindowGuid)]
    internal class HelpWindowPane : ToolWindowPane {
        internal const string WindowGuid = "9E909526-A616-43B2-A82B-FD639DCD40CB";
        private static bool _showDefaultPage;

        /// <summary>
        /// Holds browser control. When R session is restarted
        /// it is necessary to re-create the browser control since
        /// help server changes port and current links stop working.
        /// However, VS tool window doesn't like its child root
        /// control changing so instead we keed content control 
        /// unchanged and only replace browser that is inside it.
        /// </summary>
        private ContentControl _windowContentControl;

        /// <summary>
        /// Browser that displays help content
        /// </summary>
        public WebBrowser Browser { get; private set; }

        private IRSessionProvider _sessionProvider;
        private IRSession _session;

        public HelpWindowPane() {
            _sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            _session = _sessionProvider.Current;
            ConnectToSessionChangeEvents();

            Caption = Resources.HelpWindowCaption;
            BitmapImageMoniker = KnownMonikers.StatusHelp;

            _windowContentControl = new ContentControl();
            Content = _windowContentControl;

            CreateBrowser(_showDefaultPage);

            this.ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.helpWindowToolBarId);
            Controller c = new Controller();
            c.AddCommandSet(GetCommands());
            this.ToolBarCommandTarget = new CommandTargetToOleShim(null, c);
        }

        private void ConnectToSessionChangeEvents() {
            _sessionProvider.CurrentChanged += OnCurrentSessionChanged;
            ConnectToSessionEvents();
        }

        private void DisconnectFromSessionChangeEvents() {
            if (_sessionProvider != null) {
                _sessionProvider.CurrentChanged -= OnCurrentSessionChanged;
                _sessionProvider = null;
            }
        }

        private void ConnectToSessionEvents() {
            if (_session != null) {
                _session.Disconnected += OnRSessionDisconnected;
                _session.Connected += OnRSessionConnected;
            }
        }

        private void DisconnectFromSessionEvents() {
            if (_session != null) {
                _session.Disconnected -= OnRSessionDisconnected;
                _session.Connected -= OnRSessionConnected;
                _session = null;
            }
        }

        private void OnCurrentSessionChanged(object sender, EventArgs e) {
            DisconnectFromSessionEvents();
            _session = _sessionProvider.Current;
            ConnectToSessionEvents();
        }

        private void OnRSessionConnected(object sender, EventArgs e) {
            // Event fires on a background thread
            VsAppShell.Current.DispatchOnUIThread(() => {
                CreateBrowser(showDefaultPage: false);
            });
        }

        private void OnRSessionDisconnected(object sender, EventArgs e) {
            // Event fires on a background thread
            VsAppShell.Current.DispatchOnUIThread(() => {
                CloseBrowser();
            });
        }

        private void CreateBrowser(bool showDefaultPage = false) {
            if (Browser == null) {
                Browser = new WebBrowser();
                Browser.Navigating += OnNavigating;
                Browser.Navigated += OnNavigated;

                _windowContentControl.Content = Browser;
                if (showDefaultPage) {
                    HelpHomeCommand.ShowDefaultHelpPage();
                }
            }
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e) {
            string url = e.Uri.ToString();
            if (!IsHelpUrl(url)) {
                e.Cancel = true;
                Process.Start(url);
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e) {
            // Upon vavigation we need to ask VS to update UI so 
            // Back /Forward buttons become properly enabled or disabled.
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            shell.UpdateCommandUI(1);
        }

        private void NavigateTo(string url) {
            if (Browser != null) {
                Browser.Navigate(url);
            }
        }

        public static void Navigate(string url) {
            // Filter out localhost help URL from absolute URLs
            // except when the URL is the main landing page.
            if (RToolsSettings.Current.HelpBrowser == HelpBrowserType.Automatic && IsHelpUrl(url)) {
                // When control is just being created don't navigate 
                // to the default page since it will be replaced by
                // the specific help page right away.
                _showDefaultPage = false;
                HelpWindowPane pane = ToolWindowUtilities.ShowWindowPane<HelpWindowPane>(0, focus: false);
                if (pane != null) {
                    pane.NavigateTo(url);
                    return;
                }
            }
            Process.Start(url);
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

        protected override void Dispose(bool disposing) {
            if (disposing) {
                DisconnectFromSessionEvents();
                DisconnectFromSessionChangeEvents();
                CloseBrowser();
            }
            base.Dispose(disposing);
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
