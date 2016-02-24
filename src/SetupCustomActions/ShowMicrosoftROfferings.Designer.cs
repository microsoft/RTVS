namespace SetupCustomActions {
    partial class ShowMicrosoftROfferings {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.webBrowserPanel = new System.Windows.Forms.Panel();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.openInBrowser = new System.Windows.Forms.Button();
            this.webBrowserPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowserPanel
            // 
            this.webBrowserPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.webBrowserPanel.Controls.Add(this.webBrowser);
            this.webBrowserPanel.Location = new System.Drawing.Point(12, 12);
            this.webBrowserPanel.Name = "webBrowserPanel";
            this.webBrowserPanel.Size = new System.Drawing.Size(689, 439);
            this.webBrowserPanel.TabIndex = 0;
            // 
            // webBrowser
            // 
            this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser.Location = new System.Drawing.Point(0, 0);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(689, 439);
            this.webBrowser.TabIndex = 0;
            // 
            // openInBrowser
            // 
            this.openInBrowser.Location = new System.Drawing.Point(754, 660);
            this.openInBrowser.Name = "openInBrowser";
            this.openInBrowser.Size = new System.Drawing.Size(148, 31);
            this.openInBrowser.TabIndex = 1;
            this.openInBrowser.Text = "Open in Web Browser...";
            this.openInBrowser.UseVisualStyleBackColor = true;
            this.openInBrowser.Click += new System.EventHandler(this.openInBrowser_Click);
            // 
            // ShowMicrosoftROfferings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(915, 704);
            this.Controls.Add(this.openInBrowser);
            this.Controls.Add(this.webBrowserPanel);
            this.Name = "ShowMicrosoftROfferings";
            this.Text = "R Tools for Visual Studio";
            this.webBrowserPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel webBrowserPanel;
        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.Button openInBrowser;
    }
}