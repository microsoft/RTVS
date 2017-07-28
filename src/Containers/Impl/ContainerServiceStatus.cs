// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Containers {
    public struct ContainerServiceStatus {
        public bool IsServiceAvailable { get; }
        public string StatusMessage { get; }
        public int StatusCode { get; }
        public ContainerServiceStatusType StatusType { get; }

        public ContainerServiceStatus(bool serviceAvailable, string statusMessage, ContainerServiceStatusType statusType) {
            IsServiceAvailable = serviceAvailable;
            StatusMessage = statusMessage;
            StatusType = statusType;
            StatusCode = 0;
        }

        public ContainerServiceStatus(bool serviceAvailable, string statusMessage, ContainerServiceStatusType statusType, int statusCode) {
            IsServiceAvailable = serviceAvailable;
            StatusMessage = statusMessage;
            StatusType = statusType;
            StatusCode = statusCode;
        }
    }
}
