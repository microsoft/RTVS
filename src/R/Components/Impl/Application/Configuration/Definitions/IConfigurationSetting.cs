// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Components.Application.Configuration {
    /// <summary>
    /// Represents R application setting. Typically settings
    /// are stored in R file that looks like a set of assignments
    /// similar to 'setting1 &lt;- value.
    /// </summary>
    public interface IConfigurationSetting {
        /// <summary>
        /// Setting name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Setting value
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Setting category (section in the Property Grid)
        /// </summary>
        ICollection<IConfigurationSettingAttribute> Attributes { get; }

        /// <summary>
        /// Value type
        /// </summary>
        ConfigurationSettingValueType ValueType { get; set; }
    }
}
