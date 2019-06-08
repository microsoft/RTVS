// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsPlatformServices : IPlatformServices {
        public VsPlatformServices() {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            var uiShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            Assumes.Present(uiShell);
            uiShell.GetDialogOwnerHwnd(out var handle);
            ApplicationWindowHandle = handle;
        }

        public IntPtr ApplicationWindowHandle { get; }
        public string ApplicationDataFolder {
            get {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appData, @"Microsoft\RTVS");
            }
        }

        public string ApplicationFolder {
            get {
                var asmPath = Assembly.GetExecutingAssembly().GetAssemblyPath();
                return Path.GetDirectoryName(asmPath);
            }
        }
    }
}
