// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Protocol {
    public enum BrokerApiError {
        NoRInterpreters = 1,
        InterpreterNotFound,
        UnableToStartRHost,
        UnableToTerminateRHost,
        PipeAlreadyConnected,
        Win32Error
    }
}
