// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Microsoft.Common.Core.Idle;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Editor.Test.Services {
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Category.Languages.Core]
    public class IdleTaskQueueTest {
        private readonly ICoreShell _shell;

        public IdleTaskQueueTest() {
            _shell = TestCoreShell.CreateBasic();
        }

        [Test]
        public void OperationsTest() {
            var results = new List<Result>();
            var queue = new IdleTimeAsyncTaskQueue(_shell.Services);

            var ta = new TaskAction(1, results);
            queue.Enqueue(ta.Action, ta.CallBackAction, typeof(TaskAction));

            ta = new TaskAction(2, results);
            queue.Enqueue(ta.Action, ta.CallBackAction, typeof(TaskAction));

            ta = new TaskAction(3, results);
            queue.Enqueue(ta.Action, ta.CallBackAction, typeof(TaskAction));

            RunThreads();

            results.Count.Should().Be(3);
            results[0].Id.Should().Be(1);
            results[1].Id.Should().Be(2);
            results[2].Id.Should().Be(3);

            results.Clear();

            ta = new TaskAction(1, results);
            object o1 = 1;
            queue.Enqueue(ta.Action, ta.CallBackAction, o1);

            ta = new TaskAction(2, results);
            object o2 = 2;
            queue.Enqueue(ta.Action, ta.CallBackAction, o2);

            ta = new TaskAction(3, results);
            object o3 = 3;
            queue.Enqueue(ta.Action, ta.CallBackAction, o3);

            queue.IncreasePriority(o3);
            RunThreads();

            results.Count.Should().Be(3);
            results[0].Id.Should().Be(3);
            results[1].Id.Should().Be(1);
            results[2].Id.Should().Be(2);
        }

        private void RunThreads() {
            for (int i = 0; i < 10; i++) {
                var idle = _shell.GetService<IIdleTimeSource>();
                idle.DoIdle();
                Thread.Sleep(100);
            }
        }

        private class TaskAction {
            private readonly int _id;
            private readonly List<Result> _results;

            public TaskAction(int id, List<Result> results) {
                _id = id;
                _results = results;
            }
            public object Action() => new Result(_id);
            public void CallBackAction(object result) => _results.Add(result as Result);
        }

        private class Result {
            public int Id { get; }
            public Result(int id) {
                Id = id;
            }
        }
    }
}