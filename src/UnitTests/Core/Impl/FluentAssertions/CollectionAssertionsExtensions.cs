// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Execution;

namespace Microsoft.UnitTests.Core.FluentAssertions {
    public static class CollectionAssertionsExtensions {
        public static AndConstraint<GenericCollectionAssertions<TActual>> Equal<TActual, TExpected>(
            this GenericCollectionAssertions<TActual> assertions, IEnumerable<TExpected> expected, string because = "", params object[] reasonArgs)
            where TExpected : IEquatable<TActual> {
            return assertions.Equal(expected, (a, e) => e.Equals(a), because, reasonArgs);
        }

        public static AndConstraint<GenericCollectionAssertions<TActual>> StartWith<TActual, TExpected>(
            this GenericCollectionAssertions<TActual> assertions, IEnumerable<TExpected> expected, string because = "", params object[] reasonArgs)
            where TExpected : IEquatable<TActual> {
            return assertions.StartWith(expected, (a, e) => e.Equals(a), because, reasonArgs);
        }

        public static AndConstraint<GenericCollectionAssertions<TActual>> StartWith<TActual, TExpected>(
            this GenericCollectionAssertions<TActual> assertions, IEnumerable<TExpected> expectation, Func<TActual, TExpected, bool> predicate, string because = "", params object[] reasonArgs)
            where TExpected : IEquatable<TActual> {


            if (expectation == null) {
                throw new ArgumentNullException(nameof(expectation), "Cannot compare collection with <null>.");
            }

            var actualItems = assertions.Subject?.ToArray();
            var expectedItems = expectation.ToArray();

            Execute.Assertion
                .BecauseOf(because, reasonArgs)
                .WithExpectation("Expected {context:collection} to start with {0}{reason}, ", expectedItems)
                .ForCondition(!ReferenceEquals(actualItems, null))
                .FailWith("but the collection is <null>.")
                .Then
                .Given(() => actualItems)
                .AssertCollectionLength(actualItems, expectedItems)
                .Then
                .Given(items => items
                    .Take(expectedItems.Length)
                    .Select((item, i) => new { Actual = item, Expected = expectedItems[i], Index = i })
                    .FirstOrDefault(c => !predicate(c.Actual, c.Expected)))
                .ForCondition(diff => diff == null)
                .FailWith("but {1} differs at index {2}.", c => new object[] { actualItems, c.Index });

            return new AndConstraint<GenericCollectionAssertions<TActual>>(assertions);
        }

        private static ContinuationOfGiven<TActual[]> AssertCollectionLength<TActual, TExpected>(this GivenSelector<TActual[]> givenSelector, TActual[] actualItems, TExpected[] expectedItems) {
            return givenSelector.ForCondition(items => items.Length > 0)
                .FailWith("but the collection is empty.")
                .Then
                .ForCondition(items => items.Length >= expectedItems.Length)
                .FailWith("but {0} contains {1} item(s) less.", actualItems, expectedItems.Length - actualItems.Length);
        }
    }
}