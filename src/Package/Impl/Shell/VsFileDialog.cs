// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal class VsFileDialog : IFileDialog {
        private readonly ICoreShell _shell;

        public VsFileDialog(ICoreShell shell) {
            _shell = shell;
        }

        public string ShowOpenFileDialog(string filter, string initialPath = null, string title = null)
            => BrowseForFileOpen(_shell.GetDialogOwnerWindow(), filter, initialPath, title);

        public string ShowBrowseDirectoryDialog(string initialPath = null, string title = null)
            => VisualStudioTools.Dialogs.BrowseForDirectory(_shell.GetDialogOwnerWindow(), initialPath, title);

        public string ShowSaveFileDialog(string filter, string initialPath = null, string title = null)
            => BrowseForFileSave(_shell.GetDialogOwnerWindow(), filter, initialPath, title);

        private string BrowseForFileOpen(IntPtr owner, string filter, string initialPath = null, string title = null) {
            var uiShell = _shell.GetService<IVsUIShell>(typeof(SVsUIShell));
            if (uiShell == null) {
                return null;
            }

            if (owner == IntPtr.Zero) {
                ErrorHandler.ThrowOnFailure(uiShell.GetDialogOwnerHwnd(out owner));
            }

            VSOPENFILENAMEW[] openInfo = new VSOPENFILENAMEW[1];
            openInfo[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSOPENFILENAMEW));
            openInfo[0].pwzFilter = filter.Replace('|', '\0') + "\0";
            openInfo[0].hwndOwner = owner;
            openInfo[0].pwzDlgTitle = title;
            openInfo[0].nMaxFileName = 260;
            var pFileName = Marshal.AllocCoTaskMem(520);
            openInfo[0].pwzFileName = pFileName;
            openInfo[0].pwzInitialDir = Path.GetDirectoryName(initialPath);
            var nameArray = (Path.GetFileName(initialPath) + "\0").ToCharArray();
            Marshal.Copy(nameArray, 0, pFileName, nameArray.Length);

            try {
                int hr = uiShell.GetOpenFileNameViaDlg(openInfo);
                if (hr == VSConstants.OLE_E_PROMPTSAVECANCELLED) {
                    return null;
                }
                ErrorHandler.ThrowOnFailure(hr);
                return Marshal.PtrToStringAuto(openInfo[0].pwzFileName);
            } finally {
                if (pFileName != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(pFileName);
                }
            }
        }

        private string BrowseForFileSave(IntPtr owner, string filter, string initialPath = null, string title = null) {
            if (string.IsNullOrEmpty(initialPath)) {
                initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + Path.DirectorySeparatorChar;
            }

            var uiShell = _shell.GetService<IVsUIShell>(typeof(SVsUIShell));
            if (null == uiShell) {
                return null;
            }

            if (owner == IntPtr.Zero) {
                ErrorHandler.ThrowOnFailure(uiShell.GetDialogOwnerHwnd(out owner));
            }

            var saveInfo = new VSSAVEFILENAMEW[1];
            saveInfo[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSSAVEFILENAMEW));
            saveInfo[0].dwFlags = 0x00000002; // OFN_OVERWRITEPROMPT
            saveInfo[0].pwzFilter = filter.Replace('|', '\0') + "\0";
            saveInfo[0].hwndOwner = owner;
            saveInfo[0].pwzDlgTitle = title;
            saveInfo[0].nMaxFileName = 260;
            var pFileName = Marshal.AllocCoTaskMem(520);
            saveInfo[0].pwzFileName = pFileName;
            saveInfo[0].pwzInitialDir = Path.GetDirectoryName(initialPath);
            var nameArray = (Path.GetFileName(initialPath) + "\0").ToCharArray();
            Marshal.Copy(nameArray, 0, pFileName, nameArray.Length);
            try {
                int hr = uiShell.GetSaveFileNameViaDlg(saveInfo);
                if (hr == VSConstants.OLE_E_PROMPTSAVECANCELLED) {
                    return null;
                }
                ErrorHandler.ThrowOnFailure(hr);
                return Marshal.PtrToStringAuto(saveInfo[0].pwzFileName);
            } finally {
                if (pFileName != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(pFileName);
                }
            }
        }
    }
}