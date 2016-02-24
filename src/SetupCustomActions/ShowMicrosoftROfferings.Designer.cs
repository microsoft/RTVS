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
            this.closeApp = new System.Windows.Forms.Button();
            this.openInBrowser = new System.Windows.Forms.Button();
            this.webBrowserPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowserPanel
            // 
            this.webBrowserPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.webBrowserPanel.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.webBrowserPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.webBrowserPanel.Controls.Add(this.webBrowser);
            this.webBrowserPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.webBrowserPanel.Location = new System.Drawing.Point(0, 0);
            this.webBrowserPanel.Name = "webBrowserPanel";
            this.webBrowserPanel.Size = new System.Drawing.Size(991, 636);
            this.webBrowserPanel.TabIndex = 0;
            // 
            // webBrowser
            // 
            this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser.Location = new System.Drawing.Point(0, 0);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(989, 634);
            this.webBrowser.TabIndex = 0;
            // 
            // closeApp
            // 
            this.closeApp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeApp.ForeColor = System.Drawing.Color.White;
            this.closeApp.Location = new System.Drawing.Point(831, 656);
            this.closeApp.Name = "closeApp";
            this.closeApp.Size = new System.Drawing.Size(148, 32);
            this.closeApp.TabIndex = 1;
            this.closeApp.Text = "&Close";
            this.closeApp.UseVisualStyleBackColor = true;
            this.closeApp.Click += new System.EventHandler(this.closeApp_Click);
            // 
            // openInBrowser
            // 
            this.openInBrowser.BackColor = System.Drawing.Color.Black;
            this.openInBrowser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.openInBrowser.ForeColor = System.Drawing.Color.White;
            this.openInBrowser.Location = new System.Drawing.Point(665, 656);
            this.openInBrowser.Name = "openInBrowser";
            this.openInBrowser.Size = new System.Drawing.Size(148, 32);
            this.openInBrowser.TabIndex = 2;
            this.openInBrowser.Text = "&Open in Web Browser...";
            this.openInBrowser.UseVisualStyleBackColor = false;
            this.openInBrowser.Click += new System.EventHandler(this.openInBrowser_Click);
            // 
            // ShowMicrosoftROfferings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(991, 702);
            this.Controls.Add(this.openInBrowser);
            this.Controls.Add(this.closeApp);
            this.Controls.Add(this.webBrowserPanel);
            this.Name = "ShowMicrosoftROfferings";
            this.Text = "Welcome to Microsoft R Tools for Visual Studio";
            this.webBrowserPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel webBrowserPanel;
        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.Button closeApp;
        private System.Windows.Forms.Button openInBrowser;
    }
}