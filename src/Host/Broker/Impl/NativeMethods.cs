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
        public const int CREDUI_MAX_DOMAIN_LENGTH = 256;
        public const int CREDUI_MAX_PASSWORD_LENGTH = (CRED_MAX_CREDENTIAL_BLOB_SIZE / 2);

        // OS type
        const int VER_NT_WORKSTATION = 0x0000001;
        const int VER_NT_DOMAIN_CONTROLLER = 0x0000002;
        const int VER_NT_SERVER = 0x0000003;

        // Mask
        const int VER_MINORVERSION = 0x0000001;
        const int VER_MAJORVERSION = 0x0000002;
        const int VER_BUILDNUMBER = 0x0000004;
        const int VER_PLATFORMID = 0x0000008;
        const int VER_SERVICEPACKMINOR = 0x0000010;
        const int VER_SERVICEPACKMAJOR = 0x0000020;
        const int VER_SUITENAME = 0x0000040;
        const int VER_PRODUCT_TYPE = 0x0000080;

        // conditions
        const int VER_EQUAL = 1;
        const int VER_GREATER = 2;
        const int VER_GREATER_EQUAL = 3;
        const int VER_LESS = 4;
        const int VER_LESS_EQUAL = 5;
        const int VER_AND = 6;
        const int VER_OR = 7;
        const int VER_CONDITION_MASK = 7;
        const int VER_NUM_BITS_PER_CONDITION_MASK = 3;

        [StructLayout(LayoutKind.Sequential)]
        private struct OSVERSIONINFOEX {
            public uint dwOSVersionInfoSize;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public uint dwBuildNumber;
            public uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
            public UInt16 wServicePackMajor;
            public UInt16 wServicePackMinor;
            public UInt16 wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

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

        [DllImport("kernel32.dll")]
        static extern ulong VerSetConditionMask(ulong dwlConditionMask, uint dwTypeBitMask, byte dwConditionMask);

        [DllImport("kernel32.dll")]
        static extern bool VerifyVersionInfo([In] ref OSVERSIONINFOEX lpVersionInfo, uint dwTypeMask, ulong dwlConditionMask);

        public static bool IsWindowsServer() {
            OSVERSIONINFOEX osVersionInfo = default(OSVERSIONINFOEX);
            osVersionInfo.dwOSVersionInfoSize = (uint)Marshal.SizeOf(osVersionInfo);
            osVersionInfo.wProductType = VER_NT_WORKSTATION;
            ulong mask = VerSetConditionMask(0, VER_PRODUCT_TYPE, VER_EQUAL);
            return !VerifyVersionInfo(ref osVersionInfo, VER_PRODUCT_TYPE, mask);
        }
    }
}
