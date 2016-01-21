using System;
using System.Windows.Forms;

namespace SetupCustomActions {
    public partial class DSProfilePromptForm : Form {
        public bool ResetKeyboardShortcuts { get; private set; } = true;

        public DSProfilePromptForm() {
            InitializeComponent();
            this.CenterToScreen();
        }

        private void buttonYes_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void buttonNo_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void resetKeyboard_CheckedChanged(object sender, EventArgs e) {
            this.ResetKeyboardShortcuts = this.resetKeyboard.Checked;
        }
    }
}
