// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Forms;

namespace SetupCustomActions {
    public partial class YesNoMessageBox : Form {
        public YesNoMessageBox(string messageText) {
            InitializeComponent();
            this.messageText.Text = messageText;
            this.TopMost = true;
            this.CenterToScreen();
        }

        protected void buttonYes_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        protected void buttonNo_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.No;
            this.Close();
        }
    }
}
