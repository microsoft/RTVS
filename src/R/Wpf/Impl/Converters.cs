// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using Microsoft.Common.Wpf;

namespace Microsoft.R.Wpf {
    public static class Converters {
        public static IValueConverter FontScale122 { get; } = LambdaConverter.Create<double>(x => x * 1.22);
        public static IValueConverter FontScale155 { get; } = LambdaConverter.Create<double>(x => x * 1.55);
        public static IValueConverter StringJoin { get; } = LambdaConverter.Create<IEnumerable<string>>(x => string.Join(", ", x));
        public static IValueConverter NullIsTrue { get; } = LambdaConverter.Create<object>(x => x == null);
        public static IValueConverter NullIsFalse { get; } = LambdaConverter.Create<object>(x => x != null);
        public static IValueConverter TrueIsCollapsed { get; } = LambdaConverter.Create<bool>(x => x ? Visibility.Visible : Visibility.Collapsed);
        public static IValueConverter FalseIsCollapsed { get; } = LambdaConverter.Create<bool>(x => x ? Visibility.Collapsed : Visibility.Visible);
        public static IValueConverter NullOrEmptyIsCollapsed { get; } = LambdaConverter.Create<IEnumerable>(x => x == null || !x.GetEnumerator().MoveNext() ? Visibility.Collapsed : Visibility.Visible);
    }
}
