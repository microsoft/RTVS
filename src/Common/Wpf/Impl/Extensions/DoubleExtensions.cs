// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Wpf.Extensions {
    public static class DoubleExtensions {
        public static bool LessThan(this double value1, double value2) 
            => value1 < value2 && !value1.IsCloseTo(value2);

        public static bool LessOrCloseTo(this double value1, double value2)
            => value1 <= value2 || value1.IsCloseTo(value2);

        public static bool GreaterThan(this double value1, double value2) 
            => value1 > value2 && !value1.IsCloseTo(value2);

        public static bool GreaterOrCloseTo(this double value1, double value2)
            => value1 >= value2 || value1.IsCloseTo(value2);

        public static bool IsCloseTo(this double value1, double value2) => value1.IsNonreal() || value2.IsNonreal()
            ? value1.CompareTo(value2) == 0
            : Math.Abs(value1 - value2) < 1.53E-06;

        public static bool IsNonreal(this double value) => double.IsNaN(value) || double.IsInfinity(value);
    }
}