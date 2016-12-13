// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Common.Core {
    public static unsafe class NativeMethods {
        public enum LogonType : int {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
            LOGON32_LOGON_NEW_CREDENTIALS = 9,
        }

        public enum LogonProvider : int {
            LOGON32_PROVIDER_DEFAULT = 0,
            LOGON32_PROVIDER_WINNT35 = 1,
            LOGON32_PROVIDER_WINNT40 = 2,
            LOGON32_PROVIDER_WINNT50 = 3
        }

        public const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        public const uint FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        public const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        public const uint FORMAT_MESSAGE_FROM_STRING = 0x00000400;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, int dwMessageId, 
             uint dwLanguageId, ref IntPtr lpBuffer, uint nSize, IntPtr pArguments);

        [DllImport("ntdll.dll")]
        public static extern int RtlNtStatusToDosError(int Status);

        public const int MAX_PATH = 260;

        public const int CRED_MAX_USERNAME_LENGTH = 513;
        public const int CRED_MAX_CREDENTIAL_BLOB_SIZE = 512;
        public const int CREDUI_MAX_USERNAME_LENGTH = CRED_MAX_USERNAME_LENGTH;
        public const int CREDUI_MAX_PASSWORD_LENGTH = (CRED_MAX_CREDENTIAL_BLOB_SIZE / 2);

        public const int CREDUI_FLAGS_INCORRECT_PASSWORD = 0x1;
        public const int CREDUI_FLAGS_DO_NOT_PERSIST = 0x2;
        public const int CREDUI_FLAGS_REQUEST_ADMINISTRATOR = 0x4;
        public const int CREDUI_FLAGS_EXCLUDE_CERTIFICATES = 0x8;
        public const int CREDUI_FLAGS_REQUIRE_CERTIFICATE = 0x10;
        public const int CREDUI_FLAGS_SHOW_SAVE_CHECK_BOX = 0x40;
        public const int CREDUI_FLAGS_ALWAYS_SHOW_UI = 0x80;
        public const int CREDUI_FLAGS_REQUIRE_SMARTCARD = 0x100;
        public const int CREDUI_FLAGS_PASSWORD_ONLY_OK = 0x200;
        public const int CREDUI_FLAGS_VALIDATE_USERNAME = 0x400;
        public const int CREDUI_FLAGS_COMPLETE_USERNAME = 0x800;
        public const int CREDUI_FLAGS_PERSIST = 0x1000;
        public const int CREDUI_FLAGS_SERVER_CREDENTIAL = 0x4000;
        public const int CREDUI_FLAGS_EXPECT_CONFIRMATION = 0x20000;
        public const int CREDUI_FLAGS_GENERIC_CREDENTIALS = 0x40000;
        public const int CREDUI_FLAGS_USERNAME_TARGET_CREDENTIALS = 0x80000;
        public const int CREDUI_FLAGS_KEEP_USERNAME = 0x100000;

        [DllImport("credui", CharSet = CharSet.Auto)]
        public static extern int CredUIPromptForCredentials(
            ref CREDUI_INFO pUiInfo,
            string pszTargetName,
            IntPtr Reserved,
            int dwAuthError,
            StringBuilder pszUserName,
            int ulUserNameMaxChars,
            IntPtr pszPassword,
            int ulPasswordMaxChars,
            ref bool pfSave,
            int dwFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct CREDUI_INFO {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        public enum CRED_TYPE {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            MAXIMUM = 5
        }

        [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
        public static extern bool CredDelete(string target, CRED_TYPE type, int flags);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            IntPtr lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetUserProfileDirectory(
            IntPtr hToken,
            StringBuilder pszProfilePath,
            ref uint cchProfilePath);

        [DllImport("userenv.dll", CharSet = CharSet.Auto)]
        public static extern uint CreateProfile(
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserSid,
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserName,
            [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszProfilePath,
            uint cchProfilePath);

        [DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]

        public static extern bool DeleteProfile(
            [MarshalAs(UnmanagedType.LPWStr)] string lpSidString,
            [MarshalAs(UnmanagedType.LPWStr)] string lpProfilePath,
            [MarshalAs(UnmanagedType.LPWStr)] string lpComputerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint RegisterClipboardFormat(string lpszFormat);

        [DllImport("Oleaut32.dll", PreserveSig = false)]
        public static extern void VariantClear(IntPtr variant);

        [DllImport("Oleaut32.dll", PreserveSig = false)]
        public static extern void VariantInit(IntPtr variant);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public const int
            IDOK = 1,
            IDCANCEL = 2,
            IDABORT = 3,
            IDRETRY = 4,
            IDIGNORE = 5,
            IDYES = 6,
            IDNO = 7,
            IDCLOSE = 8,
            IDHELP = 9,
            IDTRYAGAIN = 10,
            IDCONTINUE = 11;

        [DllImport("shell32.dll")]
        public static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr apidl, uint dwFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr ILCreateFromPath(string fileName);

        [DllImport("shell32.dll")]
        public static extern void ILFree(IntPtr pidl);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetDriveType([MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName);

        public enum DriveType {
            /// <summary>The drive type cannot be determined.</summary>
            Unknown = 0,
            /// <summary>The root path is invalid, for example, no volume is mounted at the path.</summary>
            Error = 1,
            /// <summary>The drive is a type that has removable media, for example, a floppy drive or removable hard disk.</summary>
            Removable = 2,
            /// <summary>The drive is a type that cannot be removed, for example, a fixed hard drive.</summary>
            Fixed = 3,
            /// <summary>The drive is a remote (network) drive.</summary>
            Remote = 4,
            /// <summary>The drive is a CD-ROM drive.</summary>
            CDROM = 5,
            /// <summary>The drive is a RAM disk.</summary>
            RAMDisk = 6
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool PathIsUNC([MarshalAs(UnmanagedType.LPWStr), In] string pszPath);
    }
}
