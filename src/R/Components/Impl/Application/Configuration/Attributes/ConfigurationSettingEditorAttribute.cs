// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using static System.FormattableString;

namespace Microsoft.R.Components.Application.Configuration {
    internal sealed class ConfigurationSettingEditorAttribute: ConfigurationSettingAttributeBase {
        private static readonly Dictionary<string, string> _editorTypeMap = new Dictionary<string, string>() {
            { "ConnectionStringEditor", "ConnectionStringEditor" }
        };

        public ConfigurationSettingEditorAttribute(string editorName) : 
            base(ConfigurationSettingAttributeNames.Editor, editorName) {
            if(!_editorTypeMap.ContainsKey(editorName)) {
                new ArgumentException(Invariant($"{nameof(editorName)} does not exist"));
            }
        }

        public override Attribute GetDotNetAttribute() {
            return new EditorAttribute(_editorTypeMap[Value], typeof(UITypeEditor));
        }
    }
}
