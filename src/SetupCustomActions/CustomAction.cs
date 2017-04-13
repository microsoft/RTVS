// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using System.IO;

namespace SetupCustomActions {
    public class CustomActions {
        private const string vsVersion = "14.0";

        [CustomAction]
        public static ActionResult ShowMicrosoftROfferingsAction(Session session) {
            session.Log("Start ShowMicrosoftROfferings action");
            Process.Start("https://aka.ms/rtvs-welcome");
            session.Log("End ShowMicrosoftROfferings action");
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult UpdateMefCatalogAction(Session session) {
            session.Log("Start UpdateMefCatalogAction action");
            var appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var mefCatalog = Path.Combine(appDataLocal, @"Microsoft\VisualStudio\", vsVersion, "ComponentModelCache");
            try {
                Directory.Delete(mefCatalog, recursive: true);
            } catch (IOException) { } catch (UnauthorizedAccessException) { }

            session.Log("End UpdateMefCatalogAction action");
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
