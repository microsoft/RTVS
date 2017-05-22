// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.R.Components.PackageManager.Model {
    public class RPackage {
        public string Package { get; set; }
        public string Repository { get; set; }
        public string Description { get; set; }
        public string Depends { get; set; }
        public string Imports { get; set; }
        public string Suggests { get; set; }
        public string License { get; set; }
        public string Version { get; set; }
        public string NeedsCompilation { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string URL { get; set; }
        public string LibPath { get; set; }
        public string Priority { get; set; }
        public string LinkingTo { get; set; }
        public string Enhances { get; set; }
        [JsonProperty("License_is_FOSS")]
        public string LicenseIsFoss { get; set; }
        [JsonProperty("License_restricts_use")]
        public string LicenseRestrictsUse { get; set; }
        [JsonProperty("OS_type")]
        public string OperatingSystemType { get; set; }
        public string MD5sum { get; set; }
        public string Built { get; set; }
        public string Maintainer { get; set; }
        public string BugReports { get; set; }
        public string Published { get; set; }
    }
}
