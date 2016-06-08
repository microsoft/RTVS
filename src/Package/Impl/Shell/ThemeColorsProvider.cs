// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Common.Wpf.Themes;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.R.Package.Shell {
    [Export(typeof(IThemeColorsProvider))]
    internal sealed class ThemeColorsProvider : IThemeColorsProvider {
        public ThemeColorsProvider() {
            VSColorTheme.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(ThemeChangedEventArgs e) {
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsDarkTheme {
            get {
                var defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
                return defaultBackground.GetBrightness() < 0.5;
            }
        }
        public Color CodeBackgroundColor {
            get {
                return IsDarkTheme ? Color.FromArgb(0xFF, 0x0C, 0x0C, 0x0C) : Color.FromArgb(0xFF, 0xFA, 0xFA, 0xFA);
            }
        }

        public event EventHandler ThemeChanged;
    }
}
