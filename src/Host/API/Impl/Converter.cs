// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.API {
    public static class Converter {
        public static List<T> ToListOf<T>(this IEnumerable<object> e) {
            return new List<T>(e.Select(x => (T)Convert.ChangeType(x, typeof(T))));
        }

        public static string GetRepresentation(this object value) {
            string rvalue;
            var t = value.GetType();
            if (t == typeof(int) || t == typeof(long) || t == typeof(uint) || t == typeof(ulong) || t == typeof(float) || t == typeof(double)) {
                rvalue = Invariant($"{value}");
            } else if (t == typeof(string)) {
                rvalue = Invariant($"{((string)value).ToRStringLiteral()}'");
            } else if (t == typeof(char)) {
                rvalue = Invariant($"'{(char)value}'");
            } else if (t == typeof(RObject)) {
                rvalue = ((RObject)value).Name;
            } else if (value is IEnumerable<object>) {
                rvalue = ((IEnumerable<object>)value).ToRList();
            } else {
                throw new ArgumentException(Invariant($"Unsupported value type of {nameof(value)}: {t}"));
            }
            return rvalue;
        }

        private static string ToRList(this IEnumerable<object> list) {
            var sb = new StringBuilder();
            foreach (var o in list) {
                if (sb.Length == 0) {
                    sb.Append("c(");
                } else {
                    sb.Append(", ");
                }
                var s = o.GetRepresentation();
                sb.Append(s);
            }
            sb.Append(')');
            return sb.ToString();
        }

        public static string ToRFunctionCall(this string function, IEnumerable<RFunctionArg> arguments) {
            var sb = new StringBuilder(function);
            sb.Append('(');

            // Construct argument list
            foreach (var arg in arguments) {
                if (sb.Length > function.Length + 1) {
                    sb.Append(", ");
                }
                sb.Append(arg.Name);
                sb.Append(" = ");
                sb.Append(arg.Value);
            }
            sb.Append(')');
            return sb.ToString();
        }
    }
}
