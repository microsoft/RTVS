// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Settings;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Definitions {
    internal interface IRPackage : IPackage {
        T FindWindowPane<T>(Type t, int id, bool create) where T : ToolWindowPane;
        IEditorSettingsStorage LanguageSettingsStorage { get; }
    }
}
