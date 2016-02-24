using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace SetupCustomActions {
    public partial class ShowMicrosoftROfferings : Form {
        private int _oldWidth;
        private int _oldHeight;

        public ShowMicrosoftROfferings() {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
            webBrowser.Navigate("https://microsoft.github.io/RTVS-docs");

            _oldWidth = this.Width;
            _oldHeight = this.Height;
        }

        private void OnSizeChanged(object sender, EventArgs e) {
            int deltaX = this.Width - _oldWidth;
            int deltaY = this.Height - _oldHeight;

            int buttonsTop = this.Height - openInBrowser.Height - 16;
            int buttonsLeft = this.Width - openInBrowser.Width - closeApp.Width - 2 * 16;
            openInBrowser.SetBounds(openInBrowser.Left + deltaX, openInBrowser.Top + deltaY, openInBrowser.Width, openInBrowser.Height);
            closeApp.SetBounds(closeApp.Left + deltaX, closeApp.Top + deltaY, closeApp.Width, closeApp.Height);
            webBrowserPanel.SetBounds(0, 0, this.Width, webBrowserPanel.Height + deltaY);

            _oldWidth = this.Width;
            _oldHeight = this.Height;
        }

        private void closeApp_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void openInBrowser_Click(object sender, EventArgs e) {
            Process.Start("https://microsoft.github.io/RTVS-docs");
        }
    }
}
