// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.R.Editor.Signatures;

namespace Microsoft.R.Editor.Test.Assertions {
    [ExcludeFromCodeCoverage]
    internal class FunctionParameterAssertion : ReferenceTypeAssertions<FunctionParameter, FunctionParameterAssertion> {
        protected override string Context => "Microsoft.R.Editor.Signatures.FunctionParameter";

        public FunctionParameterAssertion(FunctionParameter parameter) {
            Subject = parameter;
        }

        public AndConstraint<FunctionParameterAssertion> HaveFunctionCall(string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.FunctionCall != null)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected ParameterInfo.FunctionCall not to be <null>{reason}.");

            return new AndConstraint<FunctionParameterAssertion>(this);
        }

        public AndConstraint<FunctionParameterAssertion> HaveParameterIndex(int index, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.ParameterIndex == index)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected ParameterInfo to have ParameterIndex {0}{reason}, but found {1}.", index, Subject.ParameterIndex);

            return new AndConstraint<FunctionParameterAssertion>(this);
        }

        public AndConstraint<FunctionParameterAssertion> HaveFunctionName(string functionName, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.FunctionName == functionName)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected ParameterInfo to have FunctionName {0}{reason}, but found {1}.", functionName, Subject.FunctionName);

            return new AndConstraint<FunctionParameterAssertion>(this);
        }

        public AndConstraint<FunctionParameterAssertion> HaveSignatureEnd(int index, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.SignatureEnd == index)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected ParameterInfo to have SignatureEnd {0}{reason}, but found {1}.", index, Subject.SignatureEnd);

            return new AndConstraint<FunctionParameterAssertion>(this);
        }
    }
}