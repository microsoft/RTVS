// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Editor.Test.Services {
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class IdleTaskQueueTest {
        private readonly IExportProvider _exportProvider;
        private readonly IEditorShell _editorShell;

        public IdleTaskQueueTest(IExportProvider exportProvider) {
            _exportProvider = exportProvider;
            _editorShell = _exportProvider.GetExportedValue<IEditorShell>();
        }

        [Test]
        [Category.Languages.Core]
        public void OperationsTest() {
            var results = new List<Result>();
            var queue = new IdleTimeAsyncTaskQueue(_editorShell);

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
                _editorShell.DoIdle();
                Thread.Sleep(100);
            }
        }

        class TaskAction {
            public int Id { get; }

            private List<Result> _results;

            public TaskAction(int id, List<Result> results) {
                Id = id;
                _results = results;
            }
            public object Action() {
                return new Result(Id);
            }

            public void CallBackAction(object result) {
                _results.Add(result as Result);
            }
        }

        class Result {
            public int Id { get; }

            public Result(int id) {
                Id = id;
            }
        }
    }
}