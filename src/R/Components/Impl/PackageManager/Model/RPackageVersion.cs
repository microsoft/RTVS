using System;

namespace Microsoft.R.Components.PackageManager.Model {
    public class RPackageVersion : IComparable<RPackageVersion> {
        private readonly string _version;

        public RPackageVersion(string version) {
            _version = version;
        }

        public int CompareTo(RPackageVersion other) {
            return 0;
        }

        public override string ToString() {
            return _version;
        }
    }
}