// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Microsoft.R.Wpf.Themes;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Wpf {
    [Export(typeof(IThemeUtilities))]
    class ThemeUtilities : IThemeUtilities {
        public void SetImageBackgroundColor(DependencyObject o, object themeKey) {
            Debug.Assert(themeKey is ThemeResourceKey);
            var color = VSColorTheme.GetThemedColor(themeKey as ThemeResourceKey);
            ImageThemingUtilities.SetImageBackgroundColor(o, Color.FromArgb(color.A, color.R, color.G, color.B));
        }
        public void SetThemeScrollBars(DependencyObject o) {
            ImageThemingUtilities.SetThemeScrollBars(o, true);
        }
    }
}
