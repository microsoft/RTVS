// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Components.Application.Configuration {
    /// <summary>
    /// Represents R application settings. Typically settings
    /// are stored in R file that looks like a set of assignments
    /// similar to 'setting1 &lt;- value.
    /// </summary>
    public interface IAppSettings {
        /// <summary>
        /// Setting names
        /// </summary>
        IEnumerable<string> Names { get; }

        /// <summary>
        /// Retrieves setting value
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object GetSetting(string name);
    }
}
