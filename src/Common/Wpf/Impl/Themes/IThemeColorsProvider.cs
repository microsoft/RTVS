// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Media;

namespace Microsoft.Common.Wpf.Themes {
    public interface IThemeColorsProvider: IDisposable {
        event EventHandler ThemeChanged;

        bool IsDarkTheme { get; }

        Color CodeBackgroundColor { get; }
    }
}
