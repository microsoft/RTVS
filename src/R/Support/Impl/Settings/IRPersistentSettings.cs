// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Support.Settings {
    public interface IRPersistentSettings {
        void LoadSettings();
        void SaveSettings();
        IDictionary<string, object> ToDictionary();
    }
}
