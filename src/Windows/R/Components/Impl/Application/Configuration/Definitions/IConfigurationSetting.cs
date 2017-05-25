// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;

namespace Microsoft.R.Components.Application.Configuration {
    /// <summary>
    /// Represents R application setting. Typically settings
    /// are stored in R file that looks like a set of assignments
    /// similar to 'setting1 &lt;- value.
    /// </summary>
    public interface IConfigurationSetting: INotifyPropertyChanged {
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
        string Category { get; }

        /// <summary>
        /// Setting description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Editor type if setting provides UI editor
        /// </summary>
        string EditorType { get; }

        /// <summary>
        /// Value type
        /// </summary>
        ConfigurationSettingValueType ValueType { get; set; }
    }
}
