// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.UserProfile {
    public class RUserProfileCreatorResult : IRUserProfileCreatorResult {

        public RUserProfileCreatorResult(string profilePath, uint win32result, bool profileExists) {
            ProfilePath = profilePath;
            Win32Result = win32result;
            ProfileExists = profileExists;
        }

        public string ProfilePath { get; }

        public uint Win32Result { get; }

        public bool ProfileExists { get; }
    }
}
