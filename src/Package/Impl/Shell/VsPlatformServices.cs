// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsPlatformServices : IPlatformServices {
        public VsPlatformServices() {
            var uiShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            IntPtr handle;
            uiShell.GetDialogOwnerHwnd(out handle);
            ApplicationWindowHandle = handle;
        }

        /// <summary>
        /// Hive under HKLM that can be used by the system administrator to control
        /// certain application functionality. For example, security and privacy related
        /// features such as level of logging permitted.
        /// </summary>
        public string LocalMachineHive => @"Software\Microsoft\R Tools";

        public IntPtr ApplicationWindowHandle { get; }
    }
}
