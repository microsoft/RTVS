// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.R.Host.Broker {
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

    internal static unsafe class NativeMethods {
        public const int MAX_PATH = 260;

        public const int CRED_MAX_USERNAME_LENGTH = 513;
        public const int CRED_MAX_CREDENTIAL_BLOB_SIZE = 512;
        public const int CREDUI_MAX_USERNAME_LENGTH = CRED_MAX_USERNAME_LENGTH;
        public const int CREDUI_MAX_PASSWORD_LENGTH = (CRED_MAX_CREDENTIAL_BLOB_SIZE / 2);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [DllImport("credui.dll", CharSet = CharSet.Unicode)]
        public static extern uint CredUIParseUserName(
            string userName,
            StringBuilder user,
            int userMaxChars,
            StringBuilder domain,
            int domainMaxChars);
        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetUserProfileDirectory(IntPtr hToken, StringBuilder pszProfilePath, ref uint cchProfilePath);
    }
}
