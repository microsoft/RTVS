// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client.Host {
    public class HostConnectionInfo {
        private static readonly IRCallbacks _nullCallbacks = new NullRCallbacks();

        public string Name { get; }
        public IRCallbacks Callbacks { get; }
        public int Timeout { get; }
        public bool UseRHostCommandLineArguments { get; }
        public bool IsInteractive { get; }
        public bool PreserveSessionData { get; }

        public HostConnectionInfo(string name, IRCallbacks callbacks, bool useRHostCommandLineArguments = false, bool isInteractive = false, int timeout = 3000, bool preserveSessionData = false) {
            Name = name;
            Callbacks = callbacks ?? _nullCallbacks;
            UseRHostCommandLineArguments = useRHostCommandLineArguments;
            IsInteractive = isInteractive;
            Timeout = timeout;
            PreserveSessionData = preserveSessionData;
        }
    }
}