// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.Languages.Core.Test.Assertions {
    [ExcludeFromCodeCoverage]
    public class TokenAssertions<TTokenType> : TokenAssertions<IToken<TTokenType>, TTokenType, TokenAssertions<TTokenType>> {
        public TokenAssertions(IToken<TTokenType> token) : base(token) {}
    }

    [ExcludeFromCodeCoverage]
    public abstract class TokenAssertions<T, TTokenType, TAssertion> : ReferenceTypeAssertions<T, TokenAssertions<T, TTokenType, TAssertion>> 
        where T: IToken<TTokenType>
        where TAssertion : TokenAssertions<T, TTokenType, TAssertion> {
        protected override string Context { get; } = "Microsoft.Languages.Core.Tokens.Token";

        protected TokenAssertions(T token) {
            Subject = token;
        }

        public AndConstraint<TAssertion> Be(TTokenType tokenType, int start, int length, string because = "", params object[] reasonArgs) {
            Subject.Should().HaveType(tokenType)
                .And.StartAt(start)
                .And.HaveLength(length);

            return new AndConstraint<TAssertion>((TAssertion)this);
        }

        public AndConstraint<TAssertion> HaveType(TTokenType tokenType, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(EqualityComparer<TTokenType>.Default.Equals(Subject.TokenType, tokenType))
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected Token to have TokenType {0}{reason}, but found {1}.", tokenType, Subject.End);

            return new AndConstraint<TAssertion>((TAssertion)this);
        }

        public AndConstraint<TAssertion> StartAt(int start, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.Start == start)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected Token to have Start {0}{reason}, but found {1}.", start, Subject.End);

            return new AndConstraint<TAssertion>((TAssertion)this);
        }

        public AndConstraint<TAssertion> HaveLength(int length, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.Length == length)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected Token to have Length {0}{reason}, but found {1}.", length, Subject.End);

            return new AndConstraint<TAssertion>((TAssertion)this);
        }

        public AndConstraint<TAssertion> EndAt(int end, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.End == end)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected Token to have End {0}{reason}, but found {1}.", end, Subject.End);

            return new AndConstraint<TAssertion>((TAssertion)this);
        }
    }
}