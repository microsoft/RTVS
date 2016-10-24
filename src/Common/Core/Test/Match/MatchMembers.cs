// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions.Formatting;

namespace Microsoft.Common.Core.Test.Match {
    [ExcludeFromCodeCoverage]
    public class MatchMembers<T> : IEquatable<T> {
        private readonly MatchMembers<T> _next;
        private readonly string _memberName;
        private readonly object _expected;
        private readonly Func<T, bool> _equals;

        static MatchMembers() {
            FluentAssertions.Formatting.Formatter.AddFormatter(new Formatter());
        }

        public MatchMembers() {
            _equals = x => true;
        }

        private MatchMembers(MatchMembers<T> next, LambdaExpression memberSelector, object expected, Func<T, bool> equals) {
            if (memberSelector == null) {
                throw new ArgumentNullException("memberSelector");
            }

            // If selector is of the form x => x.Member, which is the most common kind, then shorten it to Member.
            // Otherwise, just use the whole body of the lambda.
            var member = memberSelector.Body as MemberExpression;
            if (member != null && member.Expression is ParameterExpression) {
                _memberName = member.Member.Name;
            } else {
                _memberName = memberSelector.ToString();
            }

            _next = next;
            _expected = expected;
            _equals = equals;
        }

        private static bool Matches(object expected, object actual) {
            if (expected == null) {
                return actual == null;
            } else {
                return expected.Equals(actual);
            }
        }

        public MatchMembers<T> Matching<TMember>(Expression<Func<T, TMember>> memberSelector, IEquatable<TMember> expected) =>
            new MatchMembers<T>(this, memberSelector, expected, x => Matches(expected, memberSelector.Compile()(x)));

        public MatchMembers<T> Matching<TMember>(Expression<Func<T, TMember>> memberSelector, IComparable<TMember> from, IComparable<TMember> to)
            where TMember : IComparable<TMember> =>
            Matching(memberSelector, new MatchRange<TMember>(from, to));

        public MatchMembers<T> Matching<TMember>(Expression<Func<T, TMember?>> memberSelector, IEquatable<TMember> expected)
            where TMember : struct =>
            new MatchMembers<T>(this, memberSelector, expected, x => Matches(expected, memberSelector.Compile()(x)));

        public MatchMembers<T> Matching<TMember>(Expression<Func<T, TMember?>> memberSelector, IComparable<TMember> from, IComparable<TMember> to)
            where TMember : struct, IComparable<TMember> =>
            Matching(memberSelector, new MatchRange<TMember>(from, to));

        public bool Equals(T other) =>
            other != null && _equals(other) && _next?.Equals(other) != false;

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
                if (processedObjects != null) {
                    if (processedObjects.Contains(value)) {
                        return string.Format("{{Cyclic reference to type {0} detected}}", value.GetType());
                    } else {
                        processedObjects.Add(value);
                    }
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
                    if (match._memberName != null) {
                        sb.Append(Indent(nestedPropertyLevel + 1));
                        sb.Append(match._memberName);
                        sb.Append(" = ");
                        sb.AppendLine(FluentAssertions.Formatting.Formatter.ToString(match._expected, false, processedObjects, nestedPropertyLevel + 1));
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
