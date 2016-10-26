// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Security.Principal;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using static Microsoft.R.Host.Protocol.NativeMethods;

namespace Microsoft.R.Host.Protocol {
    class RUserProfileCreatorImpl : IUserProfileServices {
        public IUserProfileCreatorResult CreateUserProfile(IUserCredentials credentails, ILogger logger) {
            IntPtr token;
            RUserProfileCreateResponse result = RUserProfileCreateResponse.Create(13, false, string.Empty);
            uint error = 0;
            if (LogonUser(credentails.Username, credentails.Domain, credentails.Password, (int)LogonType.LOGON32_LOGON_NETWORK, (int)LogonProvider.LOGON32_PROVIDER_DEFAULT, out token)) {
                WindowsIdentity winIdentity = new WindowsIdentity(token);
                StringBuilder profileDir = new StringBuilder(MAX_PATH);
                uint size = (uint)profileDir.Capacity;

                bool profileExists = false;
                error = CreateProfile(winIdentity.User.Value, credentails.Username, profileDir, size);
                // 0x800700b7 - Profile already exists.
                if (error != 0 && error != 0x800700b7) {
                    logger?.LogError(Resources.Error_UserProfileCreateFailed, credentails.Domain, credentails.Username, error);
                    result = RUserProfileCreateResponse.Blank;
                } else if (error == 0x800700b7) {
                    profileExists = true;
                    logger?.LogInformation(Resources.Info_UserProfileAlreadyExists, credentails.Domain, credentails.Username);
                } else {
                    logger?.LogInformation(Resources.Info_UserProfileCreated, credentails.Domain, credentails.Username);
                }

                profileDir = new StringBuilder(MAX_PATH * 2);
                size = (uint)profileDir.Capacity;
                if (GetUserProfileDirectory(token, profileDir, ref size)) {
                    logger?.LogInformation(Resources.Info_UserProfileDirectoryFound, credentails.Domain, credentails.Username, profileDir.ToString());
                    result = RUserProfileCreateResponse.Create(0, profileExists, profileDir.ToString());
                } else {
                    logger?.LogError(Resources.Error_UserProfileDirectoryWasNotFound, credentails.Domain, credentails.Username, Marshal.GetLastWin32Error());
                    result = RUserProfileCreateResponse.Create((uint)Marshal.GetLastWin32Error(), profileExists, profileDir.ToString());
                }
            } else {
                logger?.LogError(Resources.Error_UserLogonFailed, credentails.Domain, credentails.Username, Marshal.GetLastWin32Error());
                result = RUserProfileCreateResponse.Create((uint)Marshal.GetLastWin32Error(), false, null);
            }

            if (token != IntPtr.Zero) {
                CloseHandle(token);
            }

            return result;
        }
    }
}
