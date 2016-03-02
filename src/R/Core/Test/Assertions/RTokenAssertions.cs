// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Languages.Core.Test.Assertions;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Test.Assertions {
    [ExcludeFromCodeCoverage]
    internal class RTokenAssertions : TokenAssertions<RToken, RTokenType, RTokenAssertions> {
        public RTokenAssertions(RToken token) : base(token) {}

        public AndConstraint<RTokenAssertions> HaveSubType(RTokenSubType subType, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.SubType == subType)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected Token to have SubType {0}{reason}, but found {1}.", subType, Subject.End);

            return new AndConstraint<RTokenAssertions>(this);
        }
    }
}