// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Interpreters;
using Microsoft.Win32;

namespace Microsoft.VisualStudio.R.Package.RClient {
    internal static class MicrosoftRClient {
        private const string rtvsKey = @"SOFTWARE\Microsoft\R Tools";

        internal static string CheckMicrosoftRClientInstall(ICoreShell coreShell, IRegistry registry = null) {
            coreShell.AssertIsOnMainThread();
            registry = registry ?? new RegistryImpl();

            var rClientPath = SqlRClientInstallation.GetRClientPath(registry);
            if (!string.IsNullOrEmpty(rClientPath) && AskUserSwitchToRClient(registry)) {
                // Get R Client path
                if (MessageButtons.Yes == coreShell.ShowMessage(Resources.Prompt_MsRClientJustInstalled, MessageButtons.YesNo)) {
                    return rClientPath;
                }
            }
            return null;
        }

        private static bool AskUserSwitchToRClient(IRegistry registry = null) {
            registry = registry ?? new RegistryImpl();
            try {
                using (var hkcu = registry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64)) {
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
