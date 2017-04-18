// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Microsoft.R.Editor.Test.Assertions {
    [ExcludeFromCodeCoverage]
    internal class ParameterInfoAssertion : ReferenceTypeAssertions<ParameterInfo, ParameterInfoAssertion> {
        protected override string Context { get; } = "Microsoft.R.Editor.Signatures.ParameterInfo";

        public ParameterInfoAssertion(ParameterInfo parameterInfo) {
            Subject = parameterInfo;
        }

        public AndConstraint<ParameterInfoAssertion> HaveFunctionCall(string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.FunctionCall != null)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected ParameterInfo.FunctionCall not to be <null>{reason}.");

            return new AndConstraint<ParameterInfoAssertion>(this);
        }

        public AndConstraint<ParameterInfoAssertion> HaveParameterIndex(int index, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.ParameterIndex == index)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected ParameterInfo to have ParameterIndex {0}{reason}, but found {1}.", index, Subject.ParameterIndex);

            return new AndConstraint<ParameterInfoAssertion>(this);
        }

        public AndConstraint<ParameterInfoAssertion> HaveFunctionName(string functionName, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.FunctionName == functionName)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected ParameterInfo to have FunctionName {0}{reason}, but found {1}.", functionName, Subject.FunctionName);

            return new AndConstraint<ParameterInfoAssertion>(this);
        }

        public AndConstraint<ParameterInfoAssertion> HaveSignatureEnd(int index, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.SignatureEnd == index)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected ParameterInfo to have SignatureEnd {0}{reason}, but found {1}.", index, Subject.SignatureEnd);

            return new AndConstraint<ParameterInfoAssertion>(this);
        }
    }
}