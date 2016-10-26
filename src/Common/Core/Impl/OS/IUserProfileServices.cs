// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.Common.Core.OS {
    public interface IUserProfileServices {
        IUserProfileCreatorResult CreateUserProfile(IUserCredentials credentails, ILogger logger);
    }
}
