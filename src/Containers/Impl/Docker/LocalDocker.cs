// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Containers.Docker {
    public struct LocalDocker {
        public string Version { get; }
        public string BinPath { get; }
        public string DockerCommandPath { get;}

        public LocalDocker(string binPath, string version, string dockerCommandPath) {
            BinPath = binPath;
            Version = version;
            DockerCommandPath = dockerCommandPath;
        }
    }
}
