// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.OS {
    public interface IUserCredentials {
        string Username { get; set; }
        string Sid { get; set; }
        string Domain { get; set; }
    }
}
