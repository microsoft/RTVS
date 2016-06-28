// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;

namespace Microsoft.R.Components.Application.Configuration {
    public interface IConfigurationSettingsStorage {
        ConfigurationSettingsCollection Load(Stream stream);
        void Save(ConfigurationSettingsCollection settings, Stream stream);
    }
}
