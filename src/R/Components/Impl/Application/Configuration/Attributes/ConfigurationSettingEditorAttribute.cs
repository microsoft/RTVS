// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing.Design;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.Application.Configuration {
    /// <summary>
    /// Describes UI editor that can visually edit particular setting.
    /// This may be a color picker for a color-type property or a database
    /// connection string editor for the connection property.
    /// </summary>
    internal sealed class ConfigurationSettingEditorAttribute : ConfigurationSettingAttributeBase {
        public ConfigurationSettingEditorAttribute(string typeName) :
            base(ConfigurationSettingAttributeNames.Editor, typeName) { }

        public override Attribute GetDotNetAttribute() {
            try {
                // Protect from misspelled or missing type names of the exported editors
                return new EditorAttribute(Value, typeof(UITypeEditor));
            } catch(ArgumentException) { }
            return null;
        }

        [Export(typeof(IConfigurationSettingAttributeFactory))]
        [Name(ConfigurationSettingAttributeNames.Description)]
        internal sealed class AttributeFactory : IConfigurationSettingAttributeFactory {
            private readonly IEnumerable<Lazy<IConfigurationSettingUIEditor>> _editors;

            [ImportingConstructor]
            public AttributeFactory(IEnumerable<Lazy<IConfigurationSettingUIEditor>> editors) {
                _editors = editors;
            }

            public IConfigurationSettingAttribute CreateInstance(string value) {
                var editor = _editors?.FirstOrDefault(e => e.Value.TypeName.EqualsOrdinal(value));
                return editor != null ? new ConfigurationSettingEditorAttribute(editor.Value.TypeName) : null;
            }
        }
    }
}
