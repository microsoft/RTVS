using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities
{
    public static class FileUtilities
    {
        public static string BrowseForFileOpen(
            IntPtr owner,
            string filter,
            string initialPath = null,
            string title = null)
        {
            IVsUIShell uiShell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            if (uiShell != null)
            {
                if (owner == IntPtr.Zero)
                {
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
                try
                {
                    int hr = uiShell.GetOpenFileNameViaDlg(openInfo);
                    if (hr == VSConstants.OLE_E_PROMPTSAVECANCELLED)
                    {
                        return null;
                    }
                    ErrorHandler.ThrowOnFailure(hr);
                    return Marshal.PtrToStringAuto(openInfo[0].pwzFileName);
                }
                finally
                {
                    if (pFileName != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pFileName);
                    }
                }
            }

            return null;
        }

        public static string BrowseForFileSave(
            IntPtr owner,
            string filter,
            string initialPath = null,
            string title = null)
        {
            if (string.IsNullOrEmpty(initialPath))
            {
                initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + Path.DirectorySeparatorChar;
            }

            IVsUIShell uiShell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            if (null != uiShell)
            {
                if (owner == IntPtr.Zero)
                {
                    ErrorHandler.ThrowOnFailure(uiShell.GetDialogOwnerHwnd(out owner));
                }

                VSSAVEFILENAMEW[] saveInfo = new VSSAVEFILENAMEW[1];
                saveInfo[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSSAVEFILENAMEW));
                saveInfo[0].pwzFilter = filter.Replace('|', '\0') + "\0";
                saveInfo[0].hwndOwner = owner;
                saveInfo[0].pwzDlgTitle = title;
                saveInfo[0].nMaxFileName = 260;
                var pFileName = Marshal.AllocCoTaskMem(520);
                saveInfo[0].pwzFileName = pFileName;
                saveInfo[0].pwzInitialDir = Path.GetDirectoryName(initialPath);
                var nameArray = (Path.GetFileName(initialPath) + "\0").ToCharArray();
                Marshal.Copy(nameArray, 0, pFileName, nameArray.Length);
                try
                {
                    int hr = uiShell.GetSaveFileNameViaDlg(saveInfo);
                    if (hr == VSConstants.OLE_E_PROMPTSAVECANCELLED)
                    {
                        return null;
                    }
                    ErrorHandler.ThrowOnFailure(hr);
                    return Marshal.PtrToStringAuto(saveInfo[0].pwzFileName);
                }
                finally
                {
                    if (pFileName != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pFileName);
                    }
                }
            }

            return null;
        }
    }
}
