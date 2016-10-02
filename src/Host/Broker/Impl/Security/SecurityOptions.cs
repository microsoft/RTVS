// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Broker.Security {
    public class SecurityOptions {
        public string Secret { get; set; }
        public string AllowedGroup { get; set; } = "R Remote Users";
    }
}
