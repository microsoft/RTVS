// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Containers.Docker {
    public class LocalDocker {
        public string BinPath { get; }
        public string DockerCommandPath { get;}

        public LocalDocker(string binPath, string dockerCommandPath) {
            BinPath = binPath;
            DockerCommandPath = dockerCommandPath;
        }
    }
}
