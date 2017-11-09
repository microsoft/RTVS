// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Common.Wpf {
    public class LambdaConverter : IValueConverter, IMultiValueConverter {
        private readonly Func<object, object> lambda;
        private readonly Func<object[], object> multiLambda;

        private LambdaConverter(Func<object, object> lambda) {
            this.lambda = lambda;
            this.multiLambda = (args) => {
                Debug.Assert(args.Length == 1);
                return lambda(args[0]);
            };
        }

        private LambdaConverter(Func<object[], object> multiLambda) {
            this.multiLambda = multiLambda;
            this.lambda = (arg) => multiLambda(new[] { arg });
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => lambda(value);
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => multiLambda(values);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        public static LambdaConverter Create(Func<dynamic, object> lambda) => Create<dynamic>(lambda);

        public static LambdaConverter Create<T1>(Func<T1, object> lambda) => new LambdaConverter(arg => {
            if (arg == DependencyProperty.UnsetValue) {
                return DependencyProperty.UnsetValue;
            }

            return lambda((T1)arg);
        });

        public static LambdaConverter Create(Func<dynamic, dynamic, object> lambda) => Create<dynamic, dynamic>(lambda);

        public static LambdaConverter Create<T1, T2>(Func<T1, T2, object> lambda) => new LambdaConverter(
            (args) => {
                Debug.Assert(args.Length == 2);

                if (args[0] == DependencyProperty.UnsetValue) {
                    return DependencyProperty.UnsetValue;
                }
                if (args[1] == DependencyProperty.UnsetValue) {
                    return DependencyProperty.UnsetValue;
                }

                return lambda((T1)args[0], (T2)args[1]);
            });

        public static LambdaConverter Create(Func<dynamic, dynamic, dynamic, object> lambda) => Create<dynamic, dynamic, dynamic>(lambda);

        public static LambdaConverter Create<T1, T2, T3>(Func<T1, T2, T3, object> lambda) => new LambdaConverter(
            (args) => {
                Debug.Assert(args.Length == 3);

                if (args[0] == DependencyProperty.UnsetValue) {
                    return DependencyProperty.UnsetValue;
                }
                if (args[1] == DependencyProperty.UnsetValue) {
                    return DependencyProperty.UnsetValue;
                }
                if (args[2] == DependencyProperty.UnsetValue) {
                    return DependencyProperty.UnsetValue;
                }

                return lambda((T1)args[0], (T2)args[1], (T3)args[2]);
            });

        public static LambdaConverter CreateMulti<T>(Func<T[], object> lambda) => new LambdaConverter(args => {
            if (args.Any(t => t == DependencyProperty.UnsetValue)) {
                return DependencyProperty.UnsetValue;
            }

            return lambda(args.Cast<T>().ToArray());
        });
    }
}
