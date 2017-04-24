// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.R.Wpf.Themes;
using Microsoft.UnitTests.Core.Mef;

namespace Microsoft.R.Components.Test.Fakes.Wpf {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IThemeUtilities))]
    [PartMetadata(PartMetadataAttributeNames.SkipInEditorTestCompositionCatalog, null)]
    internal sealed class TestThemeUtilities : IThemeUtilities {
        public void SetImageBackgroundColor(DependencyObject o, object themeKey) {}
        public void SetThemeScrollBars(DependencyObject o) {}
    }
}
