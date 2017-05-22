// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.OS;

namespace Microsoft.R.Host.Protocol.Test.UserProfileServicePipe {
    class UserProfileResultMock : IUserProfileServiceResult {
        public uint Error { get; set; }

        public bool ProfileExists { get; set; }

        public string ProfilePath {
            get { return "testProfilePath"; }
            set { }
        }

        public static UserProfileResultMock Create(bool isValid, bool isExisting) {

            var result = new UserProfileResultMock();
            result.Error = (uint)(isValid ? 0 : 13);
            result.ProfileExists = isExisting;

            return result;
        }
    }
}
