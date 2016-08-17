// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Host.Broker.Interpreters {
    public class ROptions {
        public bool AutoDetect { get; set; } = true;

        public Dictionary<string, InterpreterOptions> Interpreters { get; } = new Dictionary<string, InterpreterOptions>();
    }
}
