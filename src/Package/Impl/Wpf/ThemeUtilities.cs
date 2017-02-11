// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Microsoft.R.Wpf.Themes;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Wpf {
    [Export(typeof(IThemeUtilities))]
    class ThemeUtilities : IThemeUtilities {
        public void SetImageBackgroundColor(DependencyObject o, object themeKey) {
            if (!VsAppShell.Current.IsUnitTestEnvironment) {
                Color? color = null;
                if (themeKey is ThemeResourceKey) {
                    // VS theme colors
                    var themeColor = VSColorTheme.GetThemedColor(themeKey as ThemeResourceKey);
                    color = Color.FromArgb(themeColor.A, themeColor.R, themeColor.G, themeColor.B);
                } else if (themeKey is ResourceKey) {
                    // High contrast or system colors
                    var obj = (o as FrameworkElement)?.TryFindResource(themeKey as ResourceKey);
                    if (obj is Color) {
                        color = (Color)obj;
                    }
                } else {
                    Debug.Fail("SetImageBackgroundColor: Unknown resource key type");
                }

                if (color.HasValue) {
                    ImageThemingUtilities.SetImageBackgroundColor(o, color.Value);
                }
            }
        }

        public void SetThemeScrollBars(DependencyObject o) {
            if (!VsAppShell.Current.IsUnitTestEnvironment) {
                ImageThemingUtilities.SetThemeScrollBars(o, true);
            }
        }
    }
}
