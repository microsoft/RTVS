using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.R.Actions.Utility;
using Microsoft.Win32;

namespace SetupCustomActions {
    public class CustomActions {
        private const string vsVersion = "14.0";
        private const string vsServicingKeyName = @"SOFTWARE\Microsoft\DevDiv\vs\Servicing\" + vsVersion;

        [CustomAction]
        public static ActionResult DSProfilePromptAction(Session session) {
            ActionResult actionResult = ActionResult.Success;
            DialogResult result = DialogResult.No;
            string exceptionMessage = null;
            bool resetKeyboard = false;

            // Uncomment for debugging
            // MessageBox.Show("Custom Action", "Begin!");
            session.Log("Begin Data Science profile import action");

            string ideFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft Visual Studio 14.0\Common7\IDE\");
            string installFolder = Path.Combine(ideFolder, @"Extensions\Microsoft\R Tools for Visual Studio\");

            using (var form = new DSProfilePromptForm()) {
                result = form.ShowDialog(new SetupWindowHandle());
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

        [CustomAction]
        public static ActionResult MROInstallPromptAction(Session session) {
            ActionResult actionResult = ActionResult.Success;
            DialogResult ds = DialogResult.No;

            session.Log("Begin R detection action");
            session["InstallMRO"] = "No";

            RInstallData data = RInstallation.GetInstallationData(null,
                        SupportedRVersionList.MinMajorVersion, SupportedRVersionList.MinMinorVersion,
                        SupportedRVersionList.MaxMajorVersion, SupportedRVersionList.MaxMinorVersion);

            if (data.Status != RInstallStatus.OK) {
                //MessageBox.Show("Custom Action", data.Status.ToString() + " " + data.Exception != null ? data.Exception.Message : "");
                using (var form = new InstallMROForm()) {
                    ds = form.ShowDialog(new SetupWindowHandle());
                }
            }

            if (ds == DialogResult.Yes) {
                session["InstallMRO"] = "Yes";
                RInstallation.GoToRInstallPage();
            }

            session.Log("End R detection action");
            return actionResult;
        }

        [CustomAction]
        public static ActionResult VsCommunityInstallAction(Session session) {
            ActionResult actionResult = ActionResult.UserExit;
            DialogResult ds = DialogResult.No;
            bool vsInstalled = false;
            string[] vsKeys = new string[] { @"\enterprise", @"\professional", @"\community" };

            session.Log("Begin VS detection action");
            session["InstallVS"] = "No";

            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) {
                foreach (var vsk in vsKeys) {
                    try {
                        using (var key = hklm.OpenSubKey(vsServicingKeyName + vsk)) {
                            object value = key.GetValue("Install");
                            if (value != null && ((int)value) == 1) {
                                vsInstalled = true;
                                actionResult = ActionResult.Success;
                                break;
                            }
                        }
                    } catch (Exception) { }
                }
            }

            if (!vsInstalled) {
                using (var form = new InstallVsCommunityForm()) {
                    ds = form.ShowDialog(new SetupWindowHandle());
                }
            }

            if (ds == DialogResult.Yes) {
                session["InstallVS"] = "Yes";
                Process.Start("https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx");
            }

            session.Log("End VS detection action");
            return actionResult;
        }

        class SetupWindowHandle : IWin32Window {
            public IntPtr Handle { get; }

            public SetupWindowHandle() {
                Process[] procs = Process.GetProcessesByName("rtvs");

                Process p = procs.FirstOrDefault(x => x.MainWindowHandle != IntPtr.Zero);
                if (p != null) {
                    Handle = p.MainWindowHandle;
                }
            }
        }
    }
}
