// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Microsoft.UnitTests.Core.FluentAssertions {
    public static class ObjectAssertionsExtensions {
        public static AndConstraint<ObjectAssertions> BeEither(this ObjectAssertions assertions, params object[] expected) {
            return assertions.BeEither(expected, string.Empty);
        }

        public static AndConstraint<ObjectAssertions> BeEither(this ObjectAssertions assertions, object[] expected, string because = "", params object[] reasonArgs) {
            Execute.Assertion
                .BecauseOf(because, reasonArgs)
                .ForCondition(expected.Any(e => assertions.Subject.IsSameOrEqualTo(e)))
                .FailWith("Expected {context:object} to be any of {0}{reason}, but found {1}.", expected, assertions.Subject);
            return new AndConstraint<ObjectAssertions>(assertions);
        }
    }
}