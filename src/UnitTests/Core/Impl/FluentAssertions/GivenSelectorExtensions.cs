// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;

namespace Microsoft.UnitTests.Core.FluentAssertions {
    public static class GivenSelectorExtensions {
        public static ContinuationOfGiven<IEnumerable<T>> AssertCollectionIsNotNull<T>(this GivenSelector<IEnumerable<T>> givenSelector) {
            return givenSelector
                .ForCondition(items => !ReferenceEquals(items, null))
                .FailWith("but found collection is <null>.");
        }

        public static ContinuationOfGiven<IEnumerable<T>> AssertCollectionIsNotEmpty<T>(this GivenSelector<IEnumerable<T>> givenSelector, bool isNotEmpty = true, string message = null) {
            message = message ?? "but found empty collection.";
            return givenSelector
               .ForCondition(items => items.Any() || !isNotEmpty)
               .FailWith(message);
        }

        public static ContinuationOfGiven<IEnumerable<T>> AssertCollectionIsNotNullOrEmpty<T>(this GivenSelector<IEnumerable<T>> givenSelector) {
            return givenSelector
               .AssertCollectionIsNotNull()
               .Then
               .AssertCollectionIsNotEmpty();
        }

        public static ContinuationOfGiven<T[]> AssertCollectionHasEnoughItems<T>(this GivenSelector<IEnumerable<T>> givenSelector, int length, string message = null) {
            return givenSelector
                .Given(items => items.ToArray())
                .AssertCollectionHasEnoughItems(length, message);
        }

        public static ContinuationOfGiven<T[]> AssertCollectionHasEnoughItems<T>(this GivenSelector<T[]> givenSelector, int length, string message = null) {
            message = message ?? "but {0} contains {1} item(s) less.";
            return givenSelector
                .Given(items => items.ToArray())
                .ForCondition(items => items.Length >= length)
                .FailWith(message, items => items, items => length - items.Length);
        }

        public static ContinuationOfGiven<T[]> AssertCollectionHasNotTooManyItems<T>(this GivenSelector<IEnumerable<T>> givenSelector, int length, string message = null) {
            return givenSelector
                .Given(items => items.ToArray())
                .AssertCollectionHasNotTooManyItems(length, message);
        }

        public static ContinuationOfGiven<T[]> AssertCollectionHasNotTooManyItems<T>(this GivenSelector<T[]> givenSelector, int length, string message = null) {
            message = message ?? "but {0} contains {1} item(s) too many.";
            return givenSelector
                .Given(items => items.ToArray())
                .ForCondition(items => items.Length <= length)
                .FailWith(message, items => items, items => items.Length - length);
        }

        public static ContinuationOfGiven<T[]> AssertCollectionsHaveSameCount<T>(this GivenSelector<IEnumerable<T>> givenSelector, int length) {
            return givenSelector
               .AssertCollectionIsNotEmpty(length > 0)
               .Then
               .AssertCollectionHasEnoughItems(length)
               .Then
               .AssertCollectionHasNotTooManyItems(length);
        }

        public static ContinuationOfGiven<T[]> AssertCollectionDoesNotMissItems<T>(this GivenSelector<IEnumerable<T>> givenSelector, IEnumerable<T> expected, string message = null) {
            return givenSelector
                .Given(items => items.ToArray())
                .AssertCollectionDoesNotMissItems(expected, message);
        }

        public static ContinuationOfGiven<T[]> AssertCollectionDoesNotMissItems<T>(this GivenSelector<T[]> givenSelector, IEnumerable<T> expected, string message = null) {
            message = message ?? "but could not find item(s) {0}.";
            return givenSelector
                .ForCondition(items => !expected.Except(items).Any())
                .FailWith(message, expected.Except);
        }

        public static ContinuationOfGiven<T[]> AssertDictionaryDoesNotHaveAdditionalItems<T>(this GivenSelector<IEnumerable<T>> givenSelector, IEnumerable<T> expected, string message = null) {
            return givenSelector
                .Given(items => items.ToArray())
                .AssertDictionaryDoesNotHaveAdditionalItems(expected, message);
        }

        public static ContinuationOfGiven<T[]> AssertDictionaryDoesNotHaveAdditionalItems<T>(this GivenSelector<T[]> givenSelector, IEnumerable<T> expected, string message = null) {
            message = message ?? "but found additional item(s) {0}.";
            return givenSelector
                .ForCondition(items => !items.Except(expected).Any())
                .FailWith(message, items => items.Except(expected));
        }

        public static void AssertCollectionsHaveSameItems<TActual, TExpected>(this GivenSelector<TActual[]> givenSelector, TExpected[] expected, Func<TActual[], TExpected[], int> findIndex) {
            givenSelector
                .Given(actual => new { Items = actual, Index = findIndex(actual, expected) })
                .ForCondition(diff => diff.Index == -1)
                .FailWith("but {0} differs at index {1}.", diff => diff.Items, diff => diff.Index);
        }

        public static ContinuationOfGiven<IDictionary<TKey, TValue>> AssertDictionaryIsNotNull<TKey, TValue>(this GivenSelector<Dictionary<TKey, TValue>> givenSelector, string message = null) {
            return givenSelector
                .Given<IDictionary<TKey, TValue>>(d => d)
                .AssertDictionaryIsNotNull(message);
        }

        public static ContinuationOfGiven<IDictionary<TKey, TValue>> AssertDictionaryIsNotNull<TKey, TValue>(this GivenSelector<IDictionary<TKey, TValue>> givenSelector, string message = null) {
            message = message ?? "but found dictionary is <null>.";
            return givenSelector
                .ForCondition(items => !ReferenceEquals(items, null))
                .FailWith(message);
        }

        public static ContinuationOfGiven<IDictionary<TKey, TValue>> AssertDictionaryIsNotEmpty<TKey, TValue>(this GivenSelector<Dictionary<TKey, TValue>> givenSelector, bool isNotEmpty = true, string message = null) {
            return givenSelector
                .Given<IDictionary<TKey, TValue>>(d => d)
                .AssertDictionaryIsNotEmpty(isNotEmpty, message);
        }

        public static ContinuationOfGiven<IDictionary<TKey, TValue>> AssertDictionaryIsNotEmpty<TKey, TValue>(this GivenSelector<IDictionary<TKey, TValue>> givenSelector, bool isNotEmpty = true, string message = null) {
            message = message ?? "but found empty dictionary.";
            return givenSelector
               .ForCondition(items => items.Any())
               .FailWith(message);
        }

        public static ContinuationOfGiven<IDictionary<TKey, TValue>> AssertDictionaryIsNotNullOrEmpty<TKey, TValue>(this GivenSelector<IDictionary<TKey, TValue>> givenSelector) {
            return givenSelector
               .AssertDictionaryIsNotNull()
               .Then
               .AssertDictionaryIsNotEmpty();
        }

        public static ContinuationOfGiven<IDictionary<TKey, TValue>> AssertDictionaryDoesNotMissKeys<TKey, TValue>(this GivenSelector<IDictionary<TKey, TValue>> givenSelector, IDictionary<TKey, TValue> expected, string message = null, params Func<IDictionary<TKey, TValue>, object>[] args) {
            message = message ?? "but could not find keys {0}.";
            args = (args != null && args.Length > 0) ? args : new[] { new Func<IDictionary<TKey, TValue>, object>(items => expected.Keys.Except(items.Keys)) };
            return givenSelector
                .ForCondition(items => !expected.Keys.Except(items.Keys).Any())
                .FailWith(message, args);
        } 

        public static ContinuationOfGiven<IDictionary<TKey, TValue>> AssertDictionaryDoesNotHaveAdditionalKeys<TKey, TValue>(this GivenSelector<IDictionary<TKey, TValue>> givenSelector, IDictionary<TKey, TValue> expected, string message = null, params Func<IDictionary<TKey, TValue>, object>[] args) {
            message = message ?? "but found additional keys {0}.";
            args = (args != null && args.Length > 0) ? args : new[] { new Func<IDictionary<TKey, TValue>, object>(items => items.Keys.Except(expected.Keys)) };
            return givenSelector
                .ForCondition(items => !items.Keys.Except(expected.Keys).Any())
                .FailWith(message, args);
        }

        public static void AssertDictionaryHaveSameValues<TKey, TValue>(this GivenSelector<IDictionary<TKey, TValue>> givenSelector, IDictionary<TKey, TValue> expected, string message = null) {
            message = message ?? "but {0} differs at keys {1}.";
            givenSelector
                .Given(items => new { Items = items, Keys = items.Keys.Where(key => !EqualityComparer<TValue>.Default.Equals(items[key], expected[key])).ToList() })
                .ForCondition(diff => !diff.Keys.Any())
                .FailWith(message, diff => diff.Items, diff => diff.Keys);
        }
    }
}
