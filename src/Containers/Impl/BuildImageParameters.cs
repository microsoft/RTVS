// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Containers {
    public class BuildImageParameters {
        public string Image { get; }
        public string Tag { get; }
        public string Name { get; }
        public string DockerfilePath { get; }

        public BuildImageParameters(string dockerFile, string imageName, string imageTag, string name) {
            DockerfilePath = dockerFile;
            Image = imageName;
            Tag = imageTag;
            Name = name;
        }
    }
}
