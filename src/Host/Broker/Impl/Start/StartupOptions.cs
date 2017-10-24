// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Broker.Start {
    public class StartupOptions {
        public bool IsService { get; set; }
        public string Name { get; set; }
        public string WriteServerUrlsToPipe { get; set; }
    }
}
