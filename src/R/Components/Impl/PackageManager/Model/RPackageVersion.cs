// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.PackageManager.Model {
    public class RPackageVersion : IComparable<RPackageVersion> {
        private readonly string _version;

        public RPackageVersion(string version) {
            _version = version;
        }

        public int CompareTo(RPackageVersion other) {
            // TODO: Add real implementation
            return 0;
        }

        public override string ToString() {
            return _version;
        }
    }
}