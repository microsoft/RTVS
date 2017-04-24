// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Common.Wpf;

namespace Microsoft.R.Components.Information {
    public static class Converters {

        public static IValueConverter PercentageToColor { get; } = LambdaConverter.Create<double>((x) => {
            Color c;
            if (x < 0.4) {
                c = Color.FromArgb(0xFF, 0x4C, 0xBB, 0x17);
            } else if (x < 0.7) {
                c = Colors.Yellow;
            } else {
                c = Colors.Red;
            }
            return new SolidColorBrush(c);
        });

        public static IValueConverter PercentageToWidth { get; } = LambdaConverter.Create<double>(x => Math.Max(4, x * 48));
    }
}
