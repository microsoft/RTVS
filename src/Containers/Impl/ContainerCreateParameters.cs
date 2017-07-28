// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Containers {
    public struct ContainerCreateParameters {
        public string Image { get; }
        public string Tag { get; }
        public RepositoryCredentials ImageSourceCredentials { get; }
        public string StartOptions { get; }
        public string Command { get; }

        public ContainerCreateParameters(string image, string tag) {
            Image = image;
            Tag = tag;
            StartOptions = string.Empty;
            ImageSourceCredentials = null;
            Command = string.Empty;
        }

        public ContainerCreateParameters(string image, string tag, string startOptions) {
            Image = image;
            Tag = tag;
            StartOptions = startOptions;
            ImageSourceCredentials = null;
            Command = string.Empty;
        }

        public ContainerCreateParameters(string image, string tag, string startOptions, string command) {
            Image = image;
            Tag = tag;
            StartOptions = startOptions;
            Command = command;
            ImageSourceCredentials = null;
        }

        public ContainerCreateParameters(string image, string tag, string startOptions, RepositoryCredentials imageSourceCreadentials, string command) {
            Image = image;
            Tag = tag;
            StartOptions = startOptions;
            ImageSourceCredentials = imageSourceCreadentials;
            Command = command;
        }
    }
}
