// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Security.Principal;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Extensions.Logging;
using static Microsoft.R.Host.Protocol.NativeMethods;

namespace Microsoft.R.Host.Protocol {
    class RUserProfileServicesImpl : IUserProfileServices {
        public IUserProfileCreatorResult CreateUserProfile(IUserCredentials credentials, ILogger logger) {
            IntPtr token = IntPtr.Zero;
            IntPtr password = IntPtr.Zero;
            RUserProfileCreateResponse result = RUserProfileCreateResponse.Create(13, false, string.Empty);
            uint error = 0;
            try {
                password = Marshal.SecureStringToGlobalAllocUnicode(credentials.Password);
                if (LogonUser(credentials.Username, credentials.Domain, password, (int)LogonType.LOGON32_LOGON_NETWORK, (int)LogonProvider.LOGON32_PROVIDER_DEFAULT, out token)) {
                    WindowsIdentity winIdentity = new WindowsIdentity(token);
                    StringBuilder profileDir = new StringBuilder(MAX_PATH);
                    uint size = (uint)profileDir.Capacity;

                    bool profileExists = false;
                    error = CreateProfile(winIdentity.User.Value, credentials.Username, profileDir, size);
                    // 0x800700b7 - Profile already exists.
                    if (error != 0 && error != 0x800700b7) {
                        logger?.LogError(Resources.Error_UserProfileCreateFailed, credentials.Domain, credentials.Username, error);
                        result = RUserProfileCreateResponse.Blank;
                    } else if (error == 0x800700b7) {
                        profileExists = true;
                        logger?.LogInformation(Resources.Info_UserProfileAlreadyExists, credentials.Domain, credentials.Username);
                    } else {
                        logger?.LogInformation(Resources.Info_UserProfileCreated, credentials.Domain, credentials.Username);
                    }

                    profileDir = new StringBuilder(MAX_PATH * 2);
                    size = (uint)profileDir.Capacity;
                    if (GetUserProfileDirectory(token, profileDir, ref size)) {
                        logger?.LogInformation(Resources.Info_UserProfileDirectoryFound, credentials.Domain, credentials.Username, profileDir.ToString());
                        result = RUserProfileCreateResponse.Create(0, profileExists, profileDir.ToString());
                    } else {
                        logger?.LogError(Resources.Error_UserProfileDirectoryWasNotFound, credentials.Domain, credentials.Username, Marshal.GetLastWin32Error());
                        result = RUserProfileCreateResponse.Create((uint)Marshal.GetLastWin32Error(), profileExists, profileDir.ToString());
                    }
                } else {
                    logger?.LogError(Resources.Error_UserLogonFailed, credentials.Domain, credentials.Username, Marshal.GetLastWin32Error());
                    result = RUserProfileCreateResponse.Create((uint)Marshal.GetLastWin32Error(), false, null);
                }

            } finally {
                if (token != IntPtr.Zero) {
                    CloseHandle(token);
                }

                if(password != IntPtr.Zero) {
                    Marshal.ZeroFreeGlobalAllocUnicode(password);
                }
            }
            return result;
        }

        public int DeleteUserProfile(IUserCredentials credentials, ILogger logger) {
            logger?.LogInformation(Resources.Info_DeletingUserProfile, credentials.Domain, credentials.Username);
            IntPtr token = IntPtr.Zero;
            IntPtr password = IntPtr.Zero;
            int error = 0;

            string sid=string.Empty;
            StringBuilder profileDir = new StringBuilder(MAX_PATH * 2);
            uint size = (uint)profileDir.Capacity;
            try {
                password = Marshal.SecureStringToGlobalAllocUnicode(credentials.Password);
                if (LogonUser(credentials.Username, credentials.Domain, password, (int)LogonType.LOGON32_LOGON_NETWORK, (int)LogonProvider.LOGON32_PROVIDER_DEFAULT, out token)) {
                    WindowsIdentity winIdentity = new WindowsIdentity(token);
                    if (GetUserProfileDirectory(token, profileDir, ref size) && !string.IsNullOrWhiteSpace(profileDir.ToString())) {
                        sid = winIdentity.User.Value;
                    } else {
                        error = Marshal.GetLastWin32Error();
                        logger?.LogError(Resources.Error_UserProfileDirectoryWasNotFound, credentials.Domain, credentials.Username, error);
                    }
                }
            } finally {
                if (token != IntPtr.Zero) {
                    CloseHandle(token);
                }

                if (password != IntPtr.Zero) {
                    Marshal.ZeroFreeGlobalAllocUnicode(password);
                }
            }

            if (!string.IsNullOrWhiteSpace(sid)) {
                if (DeleteProfile(sid, null, null)) {
                    logger?.LogInformation(Resources.Info_DeletedUserProfile, credentials.Domain, credentials.Username);

                } else {
                    error = Marshal.GetLastWin32Error();
                    logger?.LogError(Resources.Error_DeleteUserProfileFailed, credentials.Domain, credentials.Username, error);
                }
            }
            
            return error;
        }
    }
}
