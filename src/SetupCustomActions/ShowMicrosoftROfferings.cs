using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace SetupCustomActions {
    public partial class ShowMicrosoftROfferings : Form {
        public ShowMicrosoftROfferings() {
            InitializeComponent();
            webBrowser.Navigate("https://microsoft.github.io/RTVS-docs");
        }

        private void openInBrowser_Click(object sender, EventArgs e) {
            Process.Start("https://microsoft.github.io/RTVS-docs");
        }
    }
}
