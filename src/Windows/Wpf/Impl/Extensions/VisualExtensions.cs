// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.Common.Wpf.Extensions {
    public static class VisualExtensions {
        public static T Bind<T>(this T visual, DependencyProperty dp, string path, object source) where T : Visual {
            BindingOperations.SetBinding(visual, dp, new Binding(path) { Source = source });
            return visual;
        }

        public static T SetGridPosition<T>(this T visual, int row, int column) where T : Visual {
            visual.SetValue(Grid.RowProperty, row);
            visual.SetValue(Grid.ColumnProperty, column);
            return visual;
        }
    }
}