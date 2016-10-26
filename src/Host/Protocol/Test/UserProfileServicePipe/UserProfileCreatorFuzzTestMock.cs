// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;

namespace Microsoft.R.Host.Protocol.Test.UserProfileServicePipe {
    internal class UserProfileCreatorFuzzTestMock : IUserProfileServices {

        public UserProfileCreatorFuzzTestMock() { }

        public IUserProfileCreatorResult CreateUserProfile(IUserCredentials credMock, ILogger logger) {
            // The fuzz test generated a valid parse-able JSON for the test we fail the login
            return UserProfileResultMock.Create(false, false);
        }
    }
}
