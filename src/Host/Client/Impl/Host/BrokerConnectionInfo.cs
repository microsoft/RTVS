// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client.Host {
    public class BrokerConnectionInfo {
        private static readonly IRCallbacks _nullCallbacks = new NullRCallbacks();

        public string Name { get; }
        public IRCallbacks Callbacks { get; }
        public string RCommandLineArguments { get; }
        public int Timeout { get; }
        public bool PreserveSessionData { get; }

        public BrokerConnectionInfo(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, bool preserveSessionData = false) {
            Name = name;
            Callbacks = callbacks ?? _nullCallbacks;
            RCommandLineArguments = rCommandLineArguments;
            Timeout = timeout;
            PreserveSessionData = preserveSessionData;
        }
    }
}