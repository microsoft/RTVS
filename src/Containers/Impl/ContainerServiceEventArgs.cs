// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Containers {
    public class ContainerServiceStatus {
        public bool IsServiceAvailable { get; set; }
        public string StatusMessage { get; set; }
        public int StatusCode { get; set; }
        public ContainerServiceStatusType StatusType { get; set; }
    }
}
