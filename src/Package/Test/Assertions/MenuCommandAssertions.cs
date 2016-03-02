using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Microsoft.VisualStudio.R.Package.Test.Assertions {
    [ExcludeFromCodeCoverage]
    public class MenuCommandAssertions : ReferenceTypeAssertions<MenuCommand, MenuCommandAssertions> {
        private int _oleStatus;
        protected override string Context { get; } = "System.ComponentModel.Design.MenuCommand";

        public MenuCommandAssertions(MenuCommand command) {
            Subject = command;
            EnsureStatus();
        }

        public AndConstraint<MenuCommandAssertions> BeVisibleAndEnabled(string because = "", params object[] reasonArgs) {
            return BeVisible(because, reasonArgs).And.BeEnabled(because, reasonArgs);
        }

        public AndConstraint<MenuCommandAssertions> BeVisibleAndDisabled(string because = "", params object[] reasonArgs) {
            return BeVisible(because, reasonArgs).And.BeDisabled(because, reasonArgs);
        }

        public AndConstraint<MenuCommandAssertions> BeInvisibleAndDisabled(string because = "", params object[] reasonArgs) {
            return BeInvisible(because, reasonArgs).And.BeDisabled(because, reasonArgs);
        }

        public AndConstraint<MenuCommandAssertions> BeEnabled(string because = "", params object[] reasonArgs) {
            return AssertPropertyValue(s => s.Enabled, because, reasonArgs, "Expected command of type {0} should be enabled {reason}, but it is disabled.");
        }

        public AndConstraint<MenuCommandAssertions> BeDisabled(string because = "", params object[] reasonArgs) {
            return AssertPropertyValue(s => !s.Enabled, because, reasonArgs, "Expected command of type {0} should be disabled {reason}, but it is enabled.");
        }

        public AndConstraint<MenuCommandAssertions> BeVisible(string because = "", params object[] reasonArgs) {
            return AssertPropertyValue(s => s.Visible, because, reasonArgs, "Expected command of type {0} should be visible {reason}, but it is invisible.");
        }

        public AndConstraint<MenuCommandAssertions> BeInvisible(string because = "", params object[] reasonArgs) {
            return AssertPropertyValue(s => !s.Visible, because, reasonArgs, "Expected command of type {0} should be invisible {reason}, but it is visible.");
        }

        public AndConstraint<MenuCommandAssertions> BeChecked(string because = "", params object[] reasonArgs) {
            return AssertPropertyValue(s => s.Checked, because, reasonArgs, "Expected command of type {0} should be checked {reason}, but it is unchecked.");
        }

        public AndConstraint<MenuCommandAssertions> BeUnchecked(string because = "", params object[] reasonArgs) {
            return AssertPropertyValue(s => !s.Checked, because, reasonArgs, "Expected command of type {0} should be unchecked {reason}, but it is checked.");
        }

        public AndConstraint<MenuCommandAssertions> BeSupported(string because = "", params object[] reasonArgs) {
            return AssertPropertyValue(s => s.Supported, because, reasonArgs, "Expected command of type {0} should be supported {reason}, but it is unsupported.");
        }

        public AndConstraint<MenuCommandAssertions> BeUnsupported(string because = "", params object[] reasonArgs) {
            return AssertPropertyValue(s => !s.Supported, because, reasonArgs, "Expected command of type {0} should be unsupported {reason}, but it is supported.");
        }

        private AndConstraint<MenuCommandAssertions> AssertPropertyValue(Func<MenuCommand, bool> propertyGetter, string because, object[] reasonArgs, string message) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(propertyGetter(Subject))
                .BecauseOf(because, reasonArgs)
                .FailWith(message, Subject.GetType().Name);

            return new AndConstraint<MenuCommandAssertions>(this);
        }

        private void EnsureStatus() {
            if (_oleStatus != -1) {
                _oleStatus = Subject.OleStatus;
            }
        }
    }
}