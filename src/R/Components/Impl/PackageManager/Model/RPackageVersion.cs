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
            if (_version == other._version) {
                return 0;
            }

            if (_version == null) {
                return -1;
            }

            if (other._version == null) {
                return 1;
            }

            // R version numbers can be separated with dots and hyphens (ex: 0.3-10)
            var splitChars = new char[] { '.', '-' };

            // Compare each version part, a missing part is considered the same as 0
            // Bad values that can't be parsed are also considered as 0
            var thisParts = _version.Split(splitChars);
            var otherParts = other._version.Split(splitChars);
            var count = Math.Max(thisParts.Length, otherParts.Length);
            for (var i = 0; i < count; i++) {
                var res = ComparePart(
                    thisParts.Length > i ? thisParts[i] : null,
                    otherParts.Length > i ? otherParts[i] : null
                );
                if (res != 0) {
                    return res;
                }
            }
            return 0;
        }

        private static int ComparePart(string thisPart, string otherPart) 
            => ParsePart(thisPart).CompareTo(ParsePart(otherPart));

        private static int ParsePart(string part) {
            var val = 0;
            if (part != null) {
                int.TryParse(part, out val);
            }
            return val;
        }

        public override string ToString() => _version;
    }
}