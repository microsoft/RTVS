// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Common.Core;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    public static class Converter {
        public static List<T> ToListOf<T>(this IEnumerable<object> e) {
            return new List<T>(e.Select(x => (T)Convert.ChangeType(x, typeof(T))));
        }

        public static string ToRLiteral(this object value) {
            if (value == null) {
                return "NULL";
            }

            string rvalue;
            if (value is int || value is long || value is uint || value is ulong || value is float || value is double) {
                rvalue = Invariant($"{value}");
            } else if (value is bool) {
                rvalue = ((bool)value) ? "TRUE" : "FALSE";
            } else if (value is string) {
                var s = (string)value;
                if (s.EqualsOrdinal("...")) {
                    return "...";
                }
                rvalue = Invariant($"{s.ToRStringLiteral()}");
            } else if (value is char) {
                rvalue = Invariant($"'{(char)value}'");
            } else if (value is IEnumerable) {
                rvalue = ((IEnumerable)value).ToRListConstructor();
            } else {
                throw new ArgumentException(Invariant($"Unsupported value type of {nameof(value)}: {value.GetType()}"));
            }
            return rvalue;
        }

        public static string ToRListConstructor(this IEnumerable e) {
            var sb = new StringBuilder();
            sb.Append("c(");
            foreach (var o in e) {
                if (sb.Length > 2) {
                    sb.Append(", ");
                }
                sb.Append(o.ToRLiteral());
            }
            sb.Append(')');
            return sb.ToString();
        }

        public static string ToRDataFrameConstructor(this DataFrame df) {
            var sb = new StringBuilder();
            foreach (var list in df.Data) {
                if (sb.Length == 0) {
                    sb.Append("data.frame(");
                } else {
                    sb.Append(", ");
                }
                sb.Append(list.ToRListConstructor());
            }
            sb.Append(", stringsAsFactors=FALSE)");
            return sb.ToString();
        }

        public static string ToRFunctionCall(this string function, params object[] arguments) {
            var sb = new StringBuilder(function);
            sb.Append('(');

            // Construct argument list
            foreach (var arg in arguments) {
                if (sb.Length > function.Length + 1) {
                    sb.Append(", ");
                }

                object a = arg;
                if (arg is RFunctionArg) {
                    var r = (RFunctionArg)arg;
                    if (!string.IsNullOrEmpty(r.Name)) {
                        sb.Append(r.Name);
                        sb.Append(" = ");
                    }
                    a = r.Value;
                }
                sb.Append(a.ToRLiteral());
            }
            sb.Append(')');
            return sb.ToString();
        }
    }
}
