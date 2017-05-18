// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Testing;
using Microsoft.R.Wpf.Themes;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Wpf {
    [Export(typeof(IThemeUtilities))]
    internal sealed class ThemeUtilities : IThemeUtilities {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public ThemeUtilities(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public void SetImageBackgroundColor(DependencyObject o, object themeKey) {
            if (TestEnvironment.Current != null) {
                return;
            }

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
            }

            Debug.Assert(color.HasValue, "SetImageBackgroundColor: Unknown resource key type or color not found");
            if (color.HasValue) {
                ImageThemingUtilities.SetImageBackgroundColor(o, color.Value);
            }
        }

        public void SetThemeScrollBars(DependencyObject o) {
            if (TestEnvironment.Current == null) {
                ImageThemingUtilities.SetThemeScrollBars(o, true);
            }
        }
    }
}
