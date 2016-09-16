// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client.BrokerServices {
    public interface ICredentialsProvider {
        void UpdateCredentials();

        void OnCredentialsValidated(bool isValid);
    }
}
