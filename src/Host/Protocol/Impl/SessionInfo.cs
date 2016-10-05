// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Protocol {
    public class SessionInfo {
        public string Id { get; set; }

        public string InterpreterId { get; set; }

        public string CommandLineArguments { get; set; }

        public SessionState State { get; set; }
    }
}
