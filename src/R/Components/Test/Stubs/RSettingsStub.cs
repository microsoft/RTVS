// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.Settings;

namespace Microsoft.R.Components.Test.Stubs {
    public sealed class RSettingsStub : IRSettings {
        public bool AlwaysSaveHistory { get; set; }
        public bool ClearFilterOnAddHistory { get; set; }
        public bool MultilineHistorySelection { get; set; }
        public string RBasePath { get; set; }
        public string CranMirror { get; set; }
        public string RCommandLineArguments { get; set; }
        public string WorkingDirectory { get; set; }
    }
}
