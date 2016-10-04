// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Microsoft.UnitTests.Core.FluentAssertions {
    public sealed class TaskAssertions : ReferenceTypeAssertions<Task, TaskAssertions> {
        public TaskAssertions(Task task) {
            Subject = task;
        }

        protected override string Context { get; } = "System.Threading.Tasks.Task";

        public AndConstraint<TaskAssertions> BeCompleted(string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.IsCompleted)
                .BecauseOf(because, reasonArgs)
                .FailWith($"Expected task to be completed{{reason}}, but it is {Subject.Status}.");

            return new AndConstraint<TaskAssertions>(this);
        }

        public AndConstraint<TaskAssertions> NotBeCompleted(string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(!Subject.IsCompleted)
                .BecauseOf(because, reasonArgs)
                .FailWith($"Expected task not to be completed{{reason}}, but {GetNotBeCompletedMessage()}.");

            return new AndConstraint<TaskAssertions>(this);
        }

        private string GetNotBeCompletedMessage() { 
            var exceptions = AsyncAssertions.GetExceptions(Subject);
            switch (Subject.Status) {
                case TaskStatus.RanToCompletion:
                    return "it has run to completion successfully";
                case TaskStatus.Canceled:
                    return $"it is canceled with exception of type {exceptions[0].GetType()}: {exceptions[0].Message}";
                case TaskStatus.Faulted:
                    return $@"it is faulted with the following exceptions:
{string.Join(Environment.NewLine, exceptions.Select(e => $"    {e.GetType()}: {e.Message}"))}";
                default:
                    return string.Empty;
            }
        }

        public AndConstraint<TaskAssertions> BeRanToCompletion(string because = "", params object[] reasonArgs) 
            => AssertStatus(TaskStatus.RanToCompletion, true, because, reasonArgs, "Expected task to completed execution successfully{reason}, but it has status {0}.", Subject.Status);

        public AndConstraint<TaskAssertions> BeCanceled(string because = "", params object[] reasonArgs) 
            => AssertStatus(TaskStatus.Canceled, true, because, reasonArgs, "Expected task to be canceled{reason}, but it has status {0}.", Subject.Status);

        public AndConstraint<TaskAssertions> NotBeCanceled(string because = "", params object[] reasonArgs) 
            => AssertStatus(TaskStatus.Canceled, false, because, reasonArgs, "Expected task not to be canceled{reason}, but it has status {0}.", Subject.Status);

        public AndConstraint<TaskAssertions> BeFaulted(string because = "", params object[] reasonArgs) 
            => AssertStatus(TaskStatus.Faulted, true, because, reasonArgs, "Expected task to be faulted{reason}, but it has status {0}.", Subject.Status);

        public AndConstraint<TaskAssertions> BeFaulted<T>(string because = "", params object[] reasonArgs) where T : Exception
            => AssertStatus(TaskStatus.Faulted, false, because, reasonArgs, "Expected task to be faulted with exception of type {0}{reason}, but it has status {1}.", typeof(T), Subject.Status);

        public AndConstraint<TaskAssertions> NotBeFaulted(string because = "", params object[] reasonArgs) 
            => AssertStatus(TaskStatus.Faulted, false, because, reasonArgs, "Expected task not to be faulted{reason}, but it has status {0}.", Subject.Status);

        private AndConstraint<TaskAssertions> AssertStatus(TaskStatus status, bool hasStatus, string because, object[] reasonArgs, string message, params object[] messageArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(status == Subject.Status == hasStatus)
                .BecauseOf(because, reasonArgs)
                .FailWith(message, messageArgs);

            return new AndConstraint<TaskAssertions>(this);
        }
    }
}