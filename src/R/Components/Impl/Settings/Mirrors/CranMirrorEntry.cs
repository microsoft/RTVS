// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Settings.Mirrors {
    public sealed class CranMirrorEntry {
        public string Name { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Url { get; set; }
    }
}
