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
    /// <summary>
    /// Utility class that provides methods for data conversion between R and C#
    /// </summary>
    public static class Converter {
        /// <summary>
        /// Converts collection of objects to a list of specific type
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="e">Objects to convert</param>
        /// <returns>List of objects converted to the target type</returns>
        public static List<T> ToListOf<T>(this IEnumerable<object> e) {
            return e.Select(x => (T)Convert.ChangeType(x, typeof(T))).ToList();
        }

        /// <summary>
        /// Converts object to a R string. For example, 'bool' to "TRUE" or "FALSE".
        /// </summary>
        /// <param name="value">Object value</param>
        /// <returns>String representing object value in R syntax</returns>
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

        /// <summary>
        /// Converts collection of object to R expression that creates
        /// R list from the provided collection of .NET objects.
        /// </summary>
        /// <param name="e">Collection of objects</param>
        /// <returns>Expression in R syntax that creates list ob objects</returns>
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

        /// <summary>
        /// Converts .NET data frame object to R expression that creates
        /// R data frame from the provided .NET object.
        /// </summary>
        /// <param name="df">Data frame</param>
        /// <returns>Expression in R syntax that creates R data frame</returns>
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

        /// <summary>
        /// Constructs R function call from funtion name and arguments
        /// </summary>
        /// <param name="function">Function name</param>
        /// <param name="arguments">Function arguments</param>
        /// <returns>Expression in R syntax that invokes function with arguments</returns>
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
