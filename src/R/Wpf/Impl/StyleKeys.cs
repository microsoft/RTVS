// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Microsoft.R.Wpf {
    public static class StyleKeys {
        public static object ThemedComboStyleKey { get; set; } = typeof(ComboBox);
        public static object ScrollBarStyleKey { get; set; } = typeof(ScrollBar);
        public static object ScrollViewerStyleKey { get; set; } = typeof(ScrollViewer);
        public static object ButtonStyleKey { get; set; } = typeof(Button);
        public static object TextBoxStyleKey { get; set; } = typeof(TextBox);
    }
}