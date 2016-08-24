// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Broker.Lifetime {
    public class LifetimeOptions {
        public int? PingTimeout { get; set; }

        public int? ParentProcessID { get; set; }
    }
}
