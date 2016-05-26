// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages {
    [Guid("2c5c6e73-2855-4590-af60-f476752cd2ec")]
    internal class RunPropertyPage : WpfBasedPropertyPage {
        internal static readonly string PageName = Resources.RunPropertyPageTitle;

        protected override string PropertyPageName => PageName;

        protected override PropertyPageControl CreatePropertyPageControl() => new RunPageControl();

        protected override PropertyPageViewModel CreatePropertyPageViewModel() => new RunPageViewModel(ConfiguredProperties);
    }
}
