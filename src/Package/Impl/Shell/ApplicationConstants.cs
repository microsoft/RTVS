// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    [Export(typeof(IApplicationConstants))]
    public sealed class ApplicationConstants : IApplicationConstants {
        /// <summary>
        /// Application name to use in log, system events, etc.
        /// </summary>
        public string ApplicationName => "RTVS";

        /// <summary>
        /// Returns host locale ID
        /// </summary>
        public uint LocaleId {
            get {
                var hostLocale = VsAppShell.Current.GetGlobalService<IUIHostLocale>(typeof(SUIHostLocale));
                uint lcid;
                if (hostLocale != null && hostLocale.GetUILocale(out lcid) == VSConstants.S_OK) {
                    return lcid;
                }
                return 0;
            }
        }

        public IntPtr ApplicationWindowHandle {
            get {
                var uiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                IntPtr handle;
                uiShell.GetDialogOwnerHwnd(out handle);
                return handle;
            }
        }

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
