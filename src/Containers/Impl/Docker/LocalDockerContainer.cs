// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.R.Containers.Docker {
    public class LocalDockerContainer : IContainer {
        private static readonly Regex _containerIdMatcher64 = new Regex("[0-9a-f]{64}", RegexOptions.IgnoreCase);
        private static readonly Regex _containerIdMatcher12 = new Regex("[0-9a-f]{12}", RegexOptions.IgnoreCase);

        public string Id { get; }
        public bool IsShortId => Id.Length < 64;

        public LocalDockerContainer(string id) {
            Id = id;
        }
    }
}
