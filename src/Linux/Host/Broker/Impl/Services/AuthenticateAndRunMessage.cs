// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Host.Broker.Services {
    internal class AuthenticateAndRunMessage {
        public string Name { get; } = "AuthAndRun";
        public string Username { get; set; }
        public string Password { get; set; }
        public IEnumerable<string> Arguments { get; set; }
        public IEnumerable<string> Environment { get; set; }
        public string WorkingDirectory { get; set; }
    }
}
