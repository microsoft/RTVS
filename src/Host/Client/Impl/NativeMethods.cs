// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.R.Host.Client {
    internal static class NativeMethods {
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

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

        [DllImport("kernel32.dll")]
        public static extern int GetSystemDefaultLCID();

        [DllImport("kernel32.dll")]
        public static extern int GetOEMCP();

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

        [DllImport("credui", CharSet = CharSet.Auto)]
        public static extern int CredUIConfirmCredentials(string pszTargetName, bool bConfirm);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct CREDUI_INFO {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }
    }
}
