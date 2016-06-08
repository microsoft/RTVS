// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using Microsoft.Languages.Editor.Classification;
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

        public Color CodeBackgroundColor => VSColorTheme.GetThemedColor(EnvironmentColors.AccentDarkBrushKey);

        public event EventHandler ThemeChanged;
    }
}
