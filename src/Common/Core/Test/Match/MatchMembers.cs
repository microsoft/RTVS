// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions.Formatting;

namespace Microsoft.Common.Core.Test.Match {
    public class MatchMembers<T> : IEquatable<T> {
        private readonly MatchMembers<T> _next;
        private readonly MemberExpression _member;
        private readonly Func<T, object> _selector;
        private readonly object _value;

        static MatchMembers() {
            FluentAssertions.Formatting.Formatter.AddFormatter(new Formatter());
        }

        public MatchMembers() {
        }

        private MatchMembers(MatchMembers<T> next, LambdaExpression memberSelector, object value) {
            _member = memberSelector?.Body as MemberExpression;
            if (_member == null) {
                throw new ArgumentException("Member selector must be of the form `x => x.Member`.", "memberSelector");
            }

            _selector = Expression.Lambda<Func<T, object>>(Expression.TypeAs(_member, typeof(object)), memberSelector.Parameters[0]).Compile();
            _value = value;
            _next = next;
        }

        public MatchMembers<T> Matching<TMember>(Expression<Func<T, TMember>> memberSelector, IEquatable<TMember> value) =>
            new MatchMembers<T>(this, memberSelector, value);

        public MatchMembers<T> Matching<TMember>(Expression<Func<T, TMember>> memberSelector, IComparable<TMember> from, IComparable<TMember> to)
            where TMember : IComparable<TMember> =>
            new MatchMembers<T>(this, memberSelector, new MatchRange<TMember>(from, to));

        public MatchMembers<T> Matching<TMember>(Expression<Func<T, TMember?>> memberSelector, IEquatable<TMember> value)
            where TMember : struct =>
            new MatchMembers<T>(this, memberSelector, value);

        public MatchMembers<T> Matching<TMember>(Expression<Func<T, TMember?>> memberSelector, IComparable<TMember> from, IComparable<TMember> to)
            where TMember : struct, IComparable<TMember> =>
            new MatchMembers<T>(this, memberSelector, new MatchRange<TMember>(from, to));

        public bool Equals(T other) {
            if (other == null) {
                return false;
            } else if (_member == null) {
                return true;
            }

            object actualValue = _selector(other);
            if (_value == null) {
                if (actualValue != null) {
                    return false;
                }
            } else {
                if (!_value.Equals(actualValue)) {
                    return false;
                }
            }

            return _next?.Equals(other) ?? true;
        }

        public override int GetHashCode() {
            throw new NotSupportedException(nameof(MatchMembers<T>) + " does not support " + nameof(GetHashCode));
        }

        public override bool Equals(object obj) =>
            obj is T && Equals((T)obj);

        private class Formatter : IValueFormatter {
            private const int IndentSize = 3;

            public bool CanHandle(object value) =>
                value is MatchMembers<T>;

            public string ToString(object value, bool useLineBreaks, IList<object> processedObjects = null, int nestedPropertyLevel = 0) {
                if (processedObjects.Contains(value)) {
                    return string.Format("{{Cyclic reference to type {0} detected}}", value.GetType());
                } else {
                    processedObjects.Add(value);
                }

                var sb = new StringBuilder();
                if (useLineBreaks) {
                    sb.Append(Environment.NewLine);
                }

                if (nestedPropertyLevel == 0) {
                    sb.AppendLine();
                    sb.AppendLine();
                }

                sb.AppendLine(typeof(T).FullName + " matching ");
                sb.AppendLine(Indent(nestedPropertyLevel) + "{");

                for (var match = (MatchMembers<T>)value; match != null; match = match._next) {
                    if (match._member != null) {
                        sb.Append(Indent(nestedPropertyLevel + 1));
                        sb.Append(match._member.Member.Name);
                        sb.Append(" = ");
                        sb.AppendLine(FluentAssertions.Formatting.Formatter.ToString(match._value, false, processedObjects, nestedPropertyLevel + 1));
                    }
                }

                sb.AppendLine(Indent(nestedPropertyLevel) + "}");
                return sb.ToString();
            }

            private static string Indent(int level) {
                return new string(' ', level * IndentSize);
            }
        }
    }
}
