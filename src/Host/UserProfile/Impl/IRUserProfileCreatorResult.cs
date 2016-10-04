// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.UserProfile {
    internal interface IRUserProfileCreatorResult {
        string ProfilePath { get; }

        uint Win32Result { get; }

        bool ProfileExists { get; }
    }
}
