// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Docker.DotNet.Models;

namespace Microsoft.R.Containers {
    public class ContainerCreateParameters {
        public string Image { get; set; }
        public string Tag { get; set; }
        public string ImageSource { get; set; }
        public AuthConfig ImageSourceAuth { get; set; }
        public string StartOptions { get; set; }
        public string Command { get; set; }
    }
}
