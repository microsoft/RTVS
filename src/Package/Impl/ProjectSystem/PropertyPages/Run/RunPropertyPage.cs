// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages {
    [Guid("2C5C6E73-2855-4590-AF60-F476752CD2EC")]
    internal class RunPropertyPage : WpfBasedPropertyPage {
        internal static readonly string PageName = Resources.ProjectProperties_RunPageTitle;

        protected override string PropertyPageName => PageName;

        protected override PropertyPageControl CreatePropertyPageControl() => new RunPageControl();

        protected override PropertyPageViewModel CreatePropertyPageViewModel() => new RunPageViewModel(ConfiguredProperties);
    }
}
