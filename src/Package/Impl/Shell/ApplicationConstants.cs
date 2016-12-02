// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed class ApplicationConstants : IApplicationConstants {
        /// <summary>
        /// Application name to use in log, system events, etc.
        /// </summary>
        public string ApplicationName => "RTVS";

        public ApplicationConstants() {
            var hostLocale = ServiceProvider.GlobalProvider.GetService(typeof(SUIHostLocale)) as IUIHostLocale;
            uint lcid;
            hostLocale.GetUILocale(out lcid);
            LocaleId = lcid;

            var uiShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            IntPtr handle;
            uiShell.GetDialogOwnerHwnd(out handle);
            ApplicationWindowHandle = handle;
        }

        /// <summary>
        /// Returns host locale ID
        /// </summary>
        public uint LocaleId { get; private set; }

        public IntPtr ApplicationWindowHandle { get; private set; }

        /// <summary>
        /// Hive under HKLM that can be used by the system administrator to control
        /// certain application functionality. For example, security and privacy related
        /// features such as level of logging permitted.
        /// </summary>
        public string LocalMachineHive => @"Software\Microsoft\R Tools";

        public UIColorTheme UIColorTheme {
            get {
                var defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
                return defaultBackground.GetBrightness() < 0.5 ? UIColorTheme.Dark : UIColorTheme.Light;
            }
        }
    }
}
