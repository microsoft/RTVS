// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using FluentAssertions.Formatting;

namespace Microsoft.Common.Core.Test.Match {
    [ExcludeFromCodeCoverage]
    public class MatchAny<T> : IEquatable<T> {
        public static readonly MatchAny<T> Instance = new MatchAny<T>();

        private readonly Func<T, bool> _condition;
        private readonly Func<string> _toString;
        private readonly object _wrapped;

        static MatchAny() {
            FluentAssertions.Formatting.Formatter.AddFormatter(new Formatter());
        }

        private MatchAny(Func<T, bool> condition, object wrapped) {
            _condition = condition;
            _toString = wrapped.ToString;
            _wrapped = wrapped;
        }

        public MatchAny() {
            _condition = _ => true;
            _toString = () => "<any>";
        }

        public MatchAny(Expression<Func<T, bool>> condition) {
            _condition = condition.Compile();
            _toString = () => $"<satisfies {condition.ToString()}>";
        }

        public static MatchAny<T> ThatMatches<TOther>(IEquatable<TOther> match)
            where TOther : T  {
            return new MatchAny<T>(x => x is TOther && match.Equals((TOther)x), match);
        }

        public bool Equals(T other) =>
            _condition(other);

        public override bool Equals(object obj) =>
            (obj == null || obj is T) && Equals((T)obj);

        public override int GetHashCode() => 0;

        public override string ToString() => _toString();

        private class Formatter : IValueFormatter {
            public bool CanHandle(object value) =>
                (value as MatchAny<T>)?._wrapped != null;

            public string ToString(object value, bool useLineBreaks, IList<object> processedObjects = null, int nestedPropertyLevel = 0) =>
                FluentAssertions.Formatting.Formatter.ToString(((MatchAny<T>)value)._wrapped, useLineBreaks, processedObjects, nestedPropertyLevel);
        }
    }
}
