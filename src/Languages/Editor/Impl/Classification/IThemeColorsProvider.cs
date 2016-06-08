// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Media;

namespace Microsoft.Languages.Editor.Classification {
    public interface IThemeColorsProvider {
        event EventHandler ThemeChanged;

        Brush CodeBackgroundColor { get; }
    }
}
