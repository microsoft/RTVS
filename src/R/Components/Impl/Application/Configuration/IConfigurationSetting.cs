// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.DataTypes;

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
        /// Retrieves setting value
        /// </summary>
        RObject Value { get; }

        /// <summary>
        /// Setting category (section in the Property Grid)
        /// </summary>
        string Category { get; }

        /// <summary>
        /// Setting description (description in the Property Grid)
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Value GUI editor
        /// </summary>
        string Editor { get; }
    }
}
