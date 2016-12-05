// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Microsoft.UnitTests.Core.FluentAssertions {
    public sealed class TaskAssertions<T> : ReferenceTypeAssertions<T, TaskAssertions<T>> where T : Task {
        public TaskAssertions(T task) {
            Subject = task;
        }

        protected override string Context { get; } = "System.Threading.Tasks.Task";

        public AndConstraint<TaskAssertions<T>> BeCompleted(string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(Subject.IsCompleted)
                .BecauseOf(because, reasonArgs)
                .FailWith($"Expected task to be completed{{reason}}, but it is {Subject.Status}.");

            return new AndConstraint<TaskAssertions<T>>(this);
        }

        public AndConstraint<TaskAssertions<T>> NotBeCompleted(string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(!Subject.IsCompleted)
                .BecauseOf(because, reasonArgs)
                .FailWith($"Expected task not to be completed{{reason}}, but {GetNotBeCompletedMessage()}.");

            return new AndConstraint<TaskAssertions<T>>(this);
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
        
        public Task<AndConstraint<TaskAssertions<T>>> BeCompletedAsync(int timeout = 10000, string because = "", params object[] reasonArgs)
            => BeInTimeAsync(BeCompletedAsyncContinuation, timeout, because:because, reasonArgs:reasonArgs);
        
        public Task<AndConstraint<TaskAssertions<T>>> BeCanceledAsync(int timeout = 10000, string because = "", params object[] reasonArgs)
            => BeInTimeAsync(BeCanceledAsyncContinuation, timeout, because: because, reasonArgs: reasonArgs);
        
        public Task<AndConstraint<TaskAssertions<T>>> NotBeCompletedAsync(int timeout = 1000, string because = "", params object[] reasonArgs) 
            => BeInTimeAsync(NotBeCompletedAsyncContinuation, timeout, 1000, because, reasonArgs);

        private Task<AndConstraint<TaskAssertions<T>>> BeInTimeAsync(Func<Task<Task>, object, AndConstraint<TaskAssertions<T>>> continuation, int timeout = 10000, int debuggerTimeout = 100000, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();
            if (Debugger.IsAttached) {
                timeout = Math.Max(debuggerTimeout, timeout);
            }

            var timeoutTask = Task.Delay(timeout);
            var state = new TimeoutContinuationState(timeout, because, reasonArgs);
            return Task.WhenAny(timeoutTask, Subject)
                .ContinueWith(continuation, state, default(CancellationToken), TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private AndConstraint<TaskAssertions<T>> BeCompletedAsyncContinuation(Task<Task> task, object state) {
            var data = (TimeoutContinuationState) state;
            AssertStatus(TaskStatus.RanToCompletion, true, data.Because, data.ReasonArgs,
                "Expected task to be completed in {0} milliseconds{reason}, but it is {1}.", data.Timeout, Subject.Status);
            return new AndConstraint<TaskAssertions<T>>(this);
        }

        private AndConstraint<TaskAssertions<T>> BeCanceledAsyncContinuation(Task<Task> task, object state) {
            var data = (TimeoutContinuationState) state;
            AssertStatus(TaskStatus.Canceled, true, data.Because, data.ReasonArgs,
                "Expected task to be canceled in {0} milliseconds{reason}, but it is {1}.", data.Timeout, Subject.Status);
            return new AndConstraint<TaskAssertions<T>>(this);
        }

        private AndConstraint<TaskAssertions<T>> NotBeCompletedAsyncContinuation(Task<Task> task, object state) {
            var data = (TimeoutContinuationState) state;
            Execute.Assertion.ForCondition(!Subject.IsCompleted)
                .BecauseOf(data.Because, data.ReasonArgs)
                .FailWith($"Expected task not to be completed in {data.Timeout} milliseconds{{reason}}, but {GetNotBeCompletedMessage()}.");

            return new AndConstraint<TaskAssertions<T>>(this);
        }

        public AndConstraint<TaskAssertions<T>> BeRanToCompletion(string because = "", params object[] reasonArgs) 
            => AssertStatus(TaskStatus.RanToCompletion, true, because, reasonArgs, "Expected task to completed execution successfully{reason}, but it has status {0}.", Subject.Status);

        public AndConstraint<TaskAssertions<T>> BeCanceled(string because = "", params object[] reasonArgs) 
            => AssertStatus(TaskStatus.Canceled, true, because, reasonArgs, "Expected task to be canceled{reason}, but it has status {0}.", Subject.Status);

        public AndConstraint<TaskAssertions<T>> NotBeCanceled(string because = "", params object[] reasonArgs) 
            => AssertStatus(TaskStatus.Canceled, false, because, reasonArgs, "Expected task not to be canceled{reason}, but it has status {0}.", Subject.Status);

        public AndConstraint<TaskAssertions<T>> BeFaulted(string because = "", params object[] reasonArgs) 
            => AssertStatus(TaskStatus.Faulted, true, because, reasonArgs, "Expected task to be faulted{reason}, but it has status {0}.", Subject.Status);

        public AndConstraint<TaskAssertions<T>> BeFaulted<TException>(string because = "", params object[] reasonArgs) where TException : Exception
            => AssertStatus(TaskStatus.Faulted, false, because, reasonArgs, "Expected task to be faulted with exception of type {0}{reason}, but it has status {1}.", typeof(TException), Subject.Status);

        public AndConstraint<TaskAssertions<T>> NotBeFaulted(string because = "", params object[] reasonArgs) 
            => AssertStatus(TaskStatus.Faulted, false, because, reasonArgs, "Expected task not to be faulted{reason}, but it has status {0}.", Subject.Status);

        private AndConstraint<TaskAssertions<T>> AssertStatus(TaskStatus status, bool hasStatus, string because, object[] reasonArgs, string message, params object[] messageArgs) {
            Subject.Should().NotBeNull();

            Execute.Assertion.ForCondition(status == Subject.Status == hasStatus)
                .BecauseOf(because, reasonArgs)
                .FailWith(message, messageArgs);

            return new AndConstraint<TaskAssertions<T>>(this);
        }

        private class TimeoutContinuationState {
            public TimeoutContinuationState(int timeout, string because, object[] reasonArgs) {
                Because = because;
                ReasonArgs = reasonArgs;
                Timeout = timeout;
            }
            public int Timeout { get; }
            public string Because { get; }
            public object[] ReasonArgs { get; }
        }
    }
}