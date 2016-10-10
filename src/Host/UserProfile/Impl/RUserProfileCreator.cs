// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.UserProfile {
    internal class RUserProfileCreator {

        internal static RUserProfileCreateResponse Create(RUserProfileCreateRequest request, ILogger logger = null) {
            IntPtr token;
            RUserProfileCreateResponse result = RUserProfileCreateResponse.Create(13, false, string.Empty);
            uint error = 0;
            if (LogonUser(request.Username, request.Domain, request.Password, (int)LogonType.LOGON32_LOGON_NETWORK, (int)LogonProvider.LOGON32_PROVIDER_DEFAULT, out token)) {
                WindowsIdentity winIdentity = new WindowsIdentity(token);
                StringBuilder profileDir = new StringBuilder(MAX_PATH);
                uint size = (uint)profileDir.Capacity;

                bool profileExists = false;
                error = CreateProfile(winIdentity.User.Value, request.Username, profileDir, size);
                // 0x800700b7 - Profile already exists.
                if (error != 0 && error != 0x800700b7) {
                    logger?.LogError(Resources.Error_UserProfileCreateFailed, request.Domain, request.Username, error);
                    result = RUserProfileCreateResponse.Blank;
                } else if (error == 0x800700b7) {
                    profileExists = true;
                    logger?.LogInformation(Resources.Info_UserProfileAlreadyExists, request.Domain, request.Username);
                } else {
                    logger?.LogInformation(Resources.Info_UserProfileCreated, request.Domain, request.Username);
                }

                profileDir = new StringBuilder(MAX_PATH * 2);
                size = (uint)profileDir.Capacity;
                if (GetUserProfileDirectory(token, profileDir, ref size)) {
                    logger?.LogInformation(Resources.Info_UserProfileDirectoryFound, request.Domain, request.Username, profileDir.ToString());
                    result = RUserProfileCreateResponse.Create(0, profileExists, profileDir.ToString());
                } else {
                    logger?.LogError(Resources.Error_UserProfileDirectoryWasNotFound, request.Domain, request.Username, Marshal.GetLastWin32Error());
                    result = RUserProfileCreateResponse.Create((uint)Marshal.GetLastWin32Error(), profileExists, profileDir.ToString());
                }
            } else {
                logger?.LogError(Resources.Error_UserLogonFailed, request.Domain, request.Username, Marshal.GetLastWin32Error());
                result = RUserProfileCreateResponse.Create((uint)Marshal.GetLastWin32Error(), false, null);
            }

            if(token != IntPtr.Zero) {
                CloseHandle(token);
            }

            return result;
        }

        const int MAX_PATH = 260;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool GetUserProfileDirectory(
            IntPtr hToken,
            StringBuilder pszProfilePath,
            ref uint cchProfilePath);

        [DllImport("userenv.dll", CharSet = CharSet.Auto)]
        static extern uint CreateProfile(
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserSid,
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserName,
            [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszProfilePath,
            uint cchProfilePath);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);
        enum LogonType : int {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
            LOGON32_LOGON_NEW_CREDENTIALS = 9,
        }

        enum LogonProvider : int {
            LOGON32_PROVIDER_DEFAULT = 0,
            LOGON32_PROVIDER_WINNT35 = 1,
            LOGON32_PROVIDER_WINNT40 = 2,
            LOGON32_PROVIDER_WINNT50 = 3
        }
    }
}
