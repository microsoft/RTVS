// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
 
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.R.Components.Test.Assertions {
    [ExcludeFromCodeCoverage]
    public class AsyncCommandAssertions : ReferenceTypeAssertions<IAsyncCommand, AsyncCommandAssertions> {
        protected override string Context { get; } = "Microsoft.R.Components.Controller.IAsyncCommand";

        public AsyncCommandAssertions(IAsyncCommand command) {
            Subject = command;
        }

        public AndConstraint<AsyncCommandAssertions> BeVisibleAndEnabled(string because = "", params object[] reasonArgs) {
            return BeVisible(because, reasonArgs).And.BeEnabled(because, reasonArgs);
        }

        public AndConstraint<AsyncCommandAssertions> BeVisibleAndDisabled(string because = "", params object[] reasonArgs) {
            return BeVisible(because, reasonArgs).And.BeDisabled(because, reasonArgs);
        }

        public AndConstraint<AsyncCommandAssertions> BeInvisibleAndDisabled(string because = "", params object[] reasonArgs) {
            return BeInvisible(because, reasonArgs).And.BeDisabled(because, reasonArgs);
        }

        public AndConstraint<AsyncCommandAssertions> BeEnabled(string because = "", params object[] reasonArgs) {
            return AssertStatus(CommandStatus.Enabled, true, because, reasonArgs, "Expected command of type {0} should be enabled {reason}, but it is disabled.");
        }

        public AndConstraint<AsyncCommandAssertions> BeDisabled(string because = "", params object[] reasonArgs) {
            return AssertStatus(CommandStatus.Enabled, false, because, reasonArgs, "Expected command of type {0} should be disabled {reason}, but it is enabled.");
        }

        public AndConstraint<AsyncCommandAssertions> BeVisible(string because = "", params object[] reasonArgs) {
            return AssertStatus(CommandStatus.Invisible, false, because, reasonArgs, "Expected command of type {0} should be visible {reason}, but it is invisible.");
        }

        public AndConstraint<AsyncCommandAssertions> BeInvisible(string because = "", params object[] reasonArgs) {
            return AssertStatus(CommandStatus.Invisible, true, because, reasonArgs, "Expected command of type {0} should be invisible {reason}, but it is visible.");
        }

        public AndConstraint<AsyncCommandAssertions> BeChecked(string because = "", params object[] reasonArgs) {
            return AssertStatus(CommandStatus.Latched, true, because, reasonArgs, "Expected command of type {0} should be checked {reason}, but it is unchecked.");
        }

        public AndConstraint<AsyncCommandAssertions> BeUnchecked(string because = "", params object[] reasonArgs) {
            return AssertStatus(CommandStatus.Latched, false, because, reasonArgs, "Expected command of type {0} should be unchecked {reason}, but it is checked.");
        }

        public AndConstraint<AsyncCommandAssertions> BeSupported(string because = "", params object[] reasonArgs) {
            return AssertStatus(CommandStatus.Supported, true, because, reasonArgs, "Expected command of type {0} should be supported {reason}, but it is unsupported.");
        }

        public AndConstraint<AsyncCommandAssertions> BeUnsupported(string because = "", params object[] reasonArgs) {
            return AssertStatus(CommandStatus.Supported, false, because, reasonArgs, "Expected command of type {0} should be unsupported {reason}, but it is supported.");
        }

        private AndConstraint<AsyncCommandAssertions> AssertStatus(CommandStatus status, bool hasStatus, string because, object[] reasonArgs, string message) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(((Subject.Status & status) == status) == hasStatus)
                .BecauseOf(because, reasonArgs)
                .FailWith(message, Subject.GetType().Name);

            return new AndConstraint<AsyncCommandAssertions>(this);
        }
    }
}