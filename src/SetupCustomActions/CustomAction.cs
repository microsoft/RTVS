using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;

namespace SetupCustomActions {
    public class CustomActions {
        [CustomAction]
        public static ActionResult DSProfilePromptAction(Session session) {
            ActionResult actionResult = ActionResult.Success;
            DialogResult result = DialogResult.No;
            string exceptionMessage = null;
            bool resetKeyboard = false;

            // Uncomment for debugging
            //MessageBox.Show("Custom Action", "Begin!");
            session.Log("Begin Data Science profile import action");

            string ideFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft Visual Studio 14.0\Common7\IDE\");
            string installFolder = Path.Combine(ideFolder, @"Extensions\Microsoft\R Tools for Visual Studio\");

            using (var form = new DSProfilePromptForm()) {
                result = form.ShowDialog();
                if (result == DialogResult.No) {
                    session.Log("User said NO");
                    actionResult = ActionResult.NotExecuted;
                }
                resetKeyboard = form.ResetKeyboardShortcuts;
            }

            if (result == DialogResult.Yes && actionResult == ActionResult.Success) {
                try {
                    session.Log("Begin importing window layout");
                    string settingsFilePath = Path.Combine(ideFolder, @"Profiles\", resetKeyboard ? "RCombined.vssettings" : "R.vssettings");

                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = Path.Combine(ideFolder, "devenv.exe");
                    psi.Arguments = string.Format(CultureInfo.InvariantCulture, "/ResetSettings \"{0}\"", settingsFilePath);
                    Process.Start(psi);
                    actionResult = ActionResult.Success;
                } catch (Exception ex) {
                    exceptionMessage = ex.Message;
                    actionResult = ActionResult.Failure;
                }
            }

            if (!string.IsNullOrEmpty(exceptionMessage)) {
                session.Log("Data Science profile import action failed. Exception: {0}", exceptionMessage);
            }

            session.Log("End Data Science profile import action");
            return actionResult;
        }
    }
}
