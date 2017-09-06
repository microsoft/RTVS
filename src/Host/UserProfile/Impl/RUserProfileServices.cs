// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.UserProfile {
    internal sealed class RUserProfileServices : IUserProfileServices {
        public IUserProfileServiceResult CreateUserProfile(IUserCredentials credentials, ILogger logger) {
            var profileDir = new StringBuilder(NativeMethods.MAX_PATH);
            var size = (uint)profileDir.Capacity;

            var profileExists = false;
            var error = NativeMethods.CreateProfile(credentials.Sid, credentials.Username, profileDir, size);
            // 0x800700b7 - Profile already exists.
            if (error != 0 && error != 0x800700b7) {
                logger?.LogError(Resources.Error_UserProfileCreateFailed, credentials.Domain, credentials.Username, error);
            } else if (error == 0x800700b7) {
                profileExists = true;
                logger?.LogInformation(Resources.Info_UserProfileAlreadyExists, credentials.Domain, credentials.Username);
            } else {
                logger?.LogInformation(Resources.Info_UserProfileCreated, credentials.Domain, credentials.Username);
            }

            return new RUserProfileServiceResponse(error, profileExists, profileDir.ToString());
        }

        public IUserProfileServiceResult DeleteUserProfile(IUserCredentials credentials, ILogger logger) {
            logger?.LogInformation(Resources.Info_DeletingUserProfile, credentials.Domain, credentials.Username);
            var error = 0;
            RUserProfileServiceResponse result;
            if (NativeMethods.DeleteProfile(credentials.Sid, null, null)) {
                logger?.LogInformation(Resources.Info_DeletedUserProfile, credentials.Domain, credentials.Username);
                result =  new RUserProfileServiceResponse((uint)error, true, "<deleted>");
            } else {
                error = Marshal.GetLastWin32Error();
                logger?.LogError(Resources.Error_DeleteUserProfileFailed, credentials.Domain, credentials.Username, error);
                result = new RUserProfileServiceResponse((uint)error, false, "");
            }

            return result;
        }
    }
}
