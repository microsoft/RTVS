// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.Win32;

namespace Microsoft.VisualStudio.R.Package.RClient {
    internal static class MicrosoftRClient {
        private const string rtvsKey = @"SOFTWARE\Microsoft\R Tools";
        private const string rClientKey = @"SOFTWARE\Microsoft\R Client";
        private const string rServer = "R_SERVER";
        private static IRegistry _registry;

        internal static IRegistry Registry {
            get {
                if (_registry == null) {
                    _registry = new RegistryImpl();
                }
                return _registry;
            }
            set { _registry = value; }
        }

        public static void CheckInstall(ICoreShell coreShell) {
            coreShell.AssertIsOnMainThread();
            var connections = coreShell.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().Connections;

            string rClientPath = CheckMicrosoftRClientInstall(coreShell);
            if (rClientPath != null) {
                connections.GetOrAddConnection("Microsoft R Client", rClientPath, string.Empty);
            }
        }

        internal static string CheckMicrosoftRClientInstall(ICoreShell coreShell) {
            coreShell.AssertIsOnMainThread();

            var rClientPath = GetRClientPath();
            if (!string.IsNullOrEmpty(rClientPath) && AskUserSwitchToRClient()) {
                // Get R Client path
                if (MessageButtons.Yes == coreShell.ShowMessage(Resources.Prompt_MsRClientJustInstalled, MessageButtons.YesNo)) {
                    return rClientPath;
                }
            }
            return null;
        }

        public static string GetRClientPath() {
            try {
                using (var hkcu = Registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                    using (var key = hkcu.OpenSubKey(rClientKey)) {
                        string path = (string)key.GetValue("Path");
                        if (!string.IsNullOrEmpty(path)) {
                            return Path.Combine(path, rServer + "\\");
                        }
                    }
                }
            } catch (Exception) { }

            return string.Empty;
        }

        private static bool AskUserSwitchToRClient() {
            try {
                using (var hkcu = Registry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64)) {
                    using (var key = hkcu.OpenSubKey(rtvsKey + "\\" + Toolset.Version, writable: true)) {
                        object value = key.GetValue("RClientPrompt");
                        if (value == null) {
                            key.SetValue("RClientPrompt", 0);
                            return true;
                        }
                    }
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) { }

            return false;
        }
    }
}
