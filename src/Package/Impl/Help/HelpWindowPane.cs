using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
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
        private WebBrowser _browser;
        private IRSessionProvider _sessionProvider;
        private IRSession _session;

        public HelpWindowPane() {
            _sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
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
            _sessionProvider.CurrentSessionChanged += OnCurrentSessionChanged;
            ConnectToSessionEvents();
        }

        private void DisconnectFromSessionChangeEvents() {
            if (_sessionProvider != null) {
                _sessionProvider.CurrentSessionChanged -= OnCurrentSessionChanged;
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
            EditorShell.Current.DispatchOnUIThread(() => {
                CreateBrowser(showDefaultPage: false);
            }, DispatcherPriority.Normal);
        }

        private void OnRSessionDisconnected(object sender, EventArgs e) {
            // Event fires on a background thread
            EditorShell.Current.DispatchOnUIThread(() => {
                CloseBrowser();
            }, DispatcherPriority.Normal);
        }

        private void CreateBrowser(bool showDefaultPage = false) {
            _browser = new WebBrowser();
            _browser.Navigating += OnNavigating;
            _browser.Navigated += OnNavigated;

            _windowContentControl.Content = _browser;
            if (showDefaultPage) {
                HelpHomeCommand.ShowDefaultHelpPage();
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
            IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            shell.UpdateCommandUI(1);
        }

        private void NavigateTo(string url) {
            if (_browser != null) {
                _browser.Navigate(url);
            }
        }

        public static void Navigate(string url) {
            // Filter out localhost help URL from absolute URLs
            // except when the URL is the main landing page.
            if (IsHelpUrl(url)) {
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
            return url.StartsWith("http://127.0.0.1");
        }

        private IEnumerable<ICommand> GetCommands() {
            List<ICommand> commands = new List<ICommand>() {
                new HelpPreviousCommand(_browser),
                new HelpNextCommand(_browser),
                new HelpHomeCommand(_browser),
                new HelpRefreshCommand(_browser)
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

            if (_browser != null) {
                _browser.Navigating -= OnNavigating;
                _browser.Navigated -= OnNavigated;
                _browser.Dispose();
                _browser = null;
            }
        }
    }
}
