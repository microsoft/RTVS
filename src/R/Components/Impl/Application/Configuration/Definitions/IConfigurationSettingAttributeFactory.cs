// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Application.Configuration {
    /// <summary>
    /// Provides factory for an attribute. Called when the attribute
    /// instance is being created. Typically when settings are being loaded from disk.
    /// Exported via MEF for a particular attribute name.
    /// </summary>
    public interface IConfigurationSettingAttributeFactory {
        IConfigurationSettingAttribute CreateInstance(string value);
    }
}
