// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.VS.Properties;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings {
    [Export(typeof(IPageMetadata))]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal class SettingsPageMetadata : IPageMetadata {
        public bool HasConfigurationCondition => true;

        public string Name => SettingsPropertyPage.PageName;

        public Guid PageGuid => typeof(SettingsPropertyPage).GUID;

        public int PageOrder => 11;
    }
}
