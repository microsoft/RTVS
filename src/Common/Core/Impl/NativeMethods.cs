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

        public const int ERROR_NOT_FOUND = 1168;

        public const int CRED_PACK_PROTECTED_CREDENTIALS = 0x1;
        public const int CRED_PACK_WOW_BUFFER = 0x2;
        public const int CRED_PACK_GENERIC_CREDENTIALS = 0x4;
        public const int CRED_PACK_ID_PROVIDER_CREDENTIALS = 0x8;

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

        public enum CRED_PERSIST : uint {
            CRED_PERSIST_SESSION = 1,
            CRED_PERSIST_LOCAL_MACHINE = 2,
            CRED_PERSIST_ENTERPRISE = 3
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CredentialData {
            public uint Flags;
            public CRED_TYPE Type;
            public string TargetName;
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public CRED_PERSIST Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredWriteW", CharSet = CharSet.Unicode)]
        internal static extern bool CredWrite(ref CredentialData userCredential, uint flags);

        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredRead(
            string target,
            CRED_TYPE type,
            int reservedFlag,
            out IntPtr userCredential);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CredFree([In] IntPtr buffer);

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

        [Flags]
        public enum CredUIWinFlags {
            /// <summary>
            /// The caller is requesting that the credential provider return the user name and password in plain text.
            /// This value cannot be combined with SECURE_PROMPT.
            /// </summary>
            CREDUIWIN_GENERIC = 0x1,
            /// <summary>
            /// The Save check box is displayed in the dialog box.
            /// </summary>
            CREDUIWIN_CHECKBOX = 0x2,
            /// <summary>
            /// Only credential providers that support the authentication package specified by the authPackage parameter should be enumerated.
            /// This value cannot be combined with CREDUIWIN_IN_CRED_ONLY.
            /// </summary>
            CREDUIWIN_AUTHPACKAGE_ONLY = 0x10,
            /// <summary>
            /// Only the credentials specified by the InAuthBuffer parameter for the authentication package specified by the authPackage parameter should be enumerated.
            /// If this flag is set, and the InAuthBuffer parameter is NULL, the function fails.
            /// This value cannot be combined with CREDUIWIN_AUTHPACKAGE_ONLY.
            /// </summary>
            CREDUIWIN_IN_CRED_ONLY = 0x20,
            /// <summary>
            /// Credential providers should enumerate only administrators. This value is intended for User Account Control (UAC) purposes only. We recommend that external callers not set this flag.
            /// </summary>
            CREDUIWIN_ENUMERATE_ADMINS = 0x100,
            /// <summary>
            /// Only the incoming credentials for the authentication package specified by the authPackage parameter should be enumerated.
            /// </summary>
            CREDUIWIN_ENUMERATE_CURRENT_USER = 0x200,
            /// <summary>
            /// The credential dialog box should be displayed on the secure desktop. This value cannot be combined with CREDUIWIN_GENERIC.
            /// Windows Vista: This value is not supported until Windows Vista with SP1.
            /// </summary>
            CREDUIWIN_SECURE_PROMPT = 0x1000,
            /// <summary>
            /// The credential provider should align the credential BLOB pointed to by the refOutAuthBuffer parameter to a 32-bit boundary, even if the provider is running on a 64-bit system.
            /// </summary>
            CREDUIWIN_PACK_32_WOW = 0x10000000,
        }

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        public static extern uint CredUIPromptForWindowsCredentials(
            ref CREDUI_INFO credInfo,
            int authError,
            ref uint authPackage,
            IntPtr InAuthBuffer,
            uint InAuthBufferSize,
            out IntPtr refOutAuthBuffer,
            out uint refOutAuthBufferSize,
            ref bool fSave,
            CredUIWinFlags flags);

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        public static extern bool CredUnPackAuthenticationBuffer(
            int dwFlags,
            IntPtr pAuthBuffer,
            uint cbAuthBuffer,
            StringBuilder pszUserName,
            ref int pcchMaxUserName,
            StringBuilder pszDomainName,
            ref int pcchMaxDomainame,
            IntPtr ptrPassword,
            ref int pcchMaxPassword);

        [DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CredPackAuthenticationBuffer(
          int dwFlags,
          string pszUserName,
          string pszPassword,
          IntPtr pPackedCredentials,
          ref int pcbPackedCredentials);
        /// <summary>
        /// Represents possible values returned by the MessageBox function.
        /// </summary>

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern uint MessageBox(IntPtr hWnd, string text, string caption, uint options);
    }
}
