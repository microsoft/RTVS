// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.R.Components.Application.Configuration {
    public interface IConfigurationSettingsReader: IDisposable {
        IReadOnlyList<IConfigurationSetting> LoadSettings();
    }
}
