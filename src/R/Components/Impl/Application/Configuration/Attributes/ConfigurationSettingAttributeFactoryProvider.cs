// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Composition;

namespace Microsoft.R.Components.Application.Configuration {
    [Export(typeof(IConfigurationSettingAttributeFactoryProvider))]
    internal sealed class ConfigurationSettingAttributeFactoryProvider : IConfigurationSettingAttributeFactoryProvider {
        [ImportMany]
        private IEnumerable<Lazy<IConfigurationSettingAttributeFactory, INamedExport>> Factories { get; set; }

        public IConfigurationSettingAttributeFactory GetFactory(string name) {
            return Factories.FirstOrDefault(e => e.Metadata.Name.EqualsIgnoreCase(name))?.Value;
        }
    }
}
