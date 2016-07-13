// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Application.Configuration {
    /// <summary>
    /// Provides UI editor for a configuration setting. Exported via MEF.
    /// <seealso cref="System.Drawing.Design.UITypeEditor"/>
    /// </summary>
    public interface IConfigurationSettingUIEditorProvider {
        Type EditorType { get; }
    }
}
