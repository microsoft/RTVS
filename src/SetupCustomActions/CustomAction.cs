using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.R.Actions.Utility;
using Microsoft.Win32;

namespace SetupCustomActions {
    public class CustomActions {
        private const string vsVersion = "14.0";
        private const string vsServicingKeyName = @"SOFTWARE\Microsoft\DevDiv\vs\Servicing\" + vsVersion;

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

        [CustomAction]
        public static ActionResult ShowMicrosoftROfferingsAction(Session session) {
            session.Log("Start ShowMicrosoftROfferings action");
            Process.Start("http://microsoft.github.io/RTVS-docs");
            session.Log("End ShowMicrosoftROfferings action");
            return ActionResult.Success;
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
