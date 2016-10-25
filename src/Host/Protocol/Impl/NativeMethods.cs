// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.R.Host.Protocol {
    internal enum LogonType : int {
        LOGON32_LOGON_INTERACTIVE = 2,
        LOGON32_LOGON_NETWORK = 3,
        LOGON32_LOGON_BATCH = 4,
        LOGON32_LOGON_SERVICE = 5,
        LOGON32_LOGON_UNLOCK = 7,
        LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
        LOGON32_LOGON_NEW_CREDENTIALS = 9,
    }

    internal enum LogonProvider : int {
        LOGON32_PROVIDER_DEFAULT = 0,
        LOGON32_PROVIDER_WINNT35 = 1,
        LOGON32_PROVIDER_WINNT40 = 2,
        LOGON32_PROVIDER_WINNT50 = 3
    }
    internal static unsafe class NativeMethods {
        internal const int MAX_PATH = 260;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool GetUserProfileDirectory(
            IntPtr hToken,
            StringBuilder pszProfilePath,
            ref uint cchProfilePath);

        [DllImport("userenv.dll", CharSet = CharSet.Auto)]
        internal static extern uint CreateProfile(
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserSid,
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserName,
            [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszProfilePath,
            uint cchProfilePath);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);
        
    }
}
