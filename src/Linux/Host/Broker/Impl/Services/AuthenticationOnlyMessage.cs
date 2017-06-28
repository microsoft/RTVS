// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Broker.Services {
    internal class AuthenticationOnlyMessage {
        public string Name { get; } = "AuthOnly";
        public string Username { get; set; }
        public string Password { get; set; }
        public string AllowedGroup { get; set; }
    }
}
