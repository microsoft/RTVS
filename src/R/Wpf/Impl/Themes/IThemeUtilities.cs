// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace Microsoft.R.Wpf.Themes {
    public interface IThemeUtilities {
        void SetImageBackgroundColor(DependencyObject o, object themeKey);
        void SetThemeScrollBars(DependencyObject o);
    }
}
