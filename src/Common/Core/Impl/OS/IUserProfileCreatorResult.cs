// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.OS {
    public interface IUserProfileCreatorResult {
        uint Error { get; set; }
        bool ProfileExists { get; set; }
        string ProfilePath { get; set; }
    }
}
