// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client.Install;
using Microsoft.R.Host.Client.Session;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.Host.Client.Test.Session {
    public partial class RSessionTest {
        public class InteractionEvaluation : IAsyncLifetime {
            private readonly TaskObserverMethodFixture _taskObserver;
            private readonly MethodInfo _testMethod;
            private readonly RSession _session;

            public InteractionEvaluation(TestMethodFixture testMethod, TaskObserverMethodFixture taskObserver) {
                _taskObserver = taskObserver;
                _testMethod = testMethod.MethodInfo;
                _session = new RSession(0, () => { });
            }

            public async Task InitializeAsync() {
                await _session.StartHostAsync(new RHostStartupInfo {
                    Name = _testMethod.Name,
                    RBasePath = new RInstallation().GetRInstallPath()
                }, null, 50000);

                _taskObserver.ObserveTaskFailure(_session.RHost.GetRHostRunTask());
            }

            public async Task DisposeAsync() {
                await _session.StopHostAsync();
                _session.Dispose();
            }

            [Test]
            [Category.R.Session]
            public async Task ExclusiveInteraction() {
                var interactionTasks = await ParallelTools.InvokeAsync(4, i => Task.Factory.StartNew(() => _session.BeginInteractionAsync()));
                IList<Task<IRSessionInteraction>> runningTasks = interactionTasks.ToList();

                while (runningTasks.Count > 0) {
                    await Task.WhenAny(runningTasks);

                    IList<Task<IRSessionInteraction>> completedTasks;
                    runningTasks.Split(t => t.Status == TaskStatus.RanToCompletion, out completedTasks, out runningTasks);
                    completedTasks.Should().ContainSingle();
                    completedTasks.Single().Result.Dispose();
                }
            }

            [Test(Skip = "https://github.com/Microsoft/RTVS/issues/1193")]
            [Category.R.Session]
            public async Task OneResponsePerInteraction() {
                using (var interaction = await _session.BeginInteractionAsync()) {
                    // ReSharper disable once AccessToDisposedClosure
                    Func<Task> f = () => interaction.RespondAsync("1+1");
                    f.ShouldNotThrow();
                    f.ShouldThrow<InvalidOperationException>();
                }
            }

            [Test]
            [Category.R.Session]
            public async Task ExclusiveEvaluation() {
                var interactionTasks = await ParallelTools.InvokeAsync(4, i => Task.Factory.StartNew(() => _session.BeginEvaluationAsync()));
                IList<Task<IRSessionEvaluation>> runningTasks = interactionTasks.ToList();

                while (runningTasks.Count > 0) {
                    await Task.WhenAny(runningTasks);

                    IList<Task<IRSessionEvaluation>> completedTasks;
                    runningTasks.Split(t => t.Status == TaskStatus.RanToCompletion, out completedTasks, out runningTasks);
                    completedTasks.Should().ContainSingle();
                    completedTasks.Single().Result.Dispose();
                }
            }

            [Test]
            [Category.R.Session]
            public async Task NestedInteraction() {
                string topLevelPrompt;
                using (var inter = await _session.BeginInteractionAsync()) {
                    topLevelPrompt = inter.Prompt;

                    var evalTask = _session.EvaluateAsync<string>("readline('2')", REvaluationKind.Reentrant);

                    using (var inter2 = await _session.BeginInteractionAsync()) {
                        inter2.Prompt.Should().Be("2");

                        var evalTask2 = _session.EvaluateAsync<string>("readline('3')", REvaluationKind.Reentrant);

                        using (var inter3 = await _session.BeginInteractionAsync()) {
                            inter3.Prompt.Should().Be("3");
                            inter3.RespondAsync("0 + 3\n").DoNotWait();
                        }

                        await evalTask2;
                        evalTask2.Result.Should().Be("0 + 3");

                        inter2.RespondAsync("0 + 2\n").DoNotWait();
                    }

                    await evalTask;
                    evalTask.Result.Should().Be("0 + 2");

                    await inter.RespondAsync("0 + 1");
                }

                using (var inter = await _session.BeginInteractionAsync()) {
                    inter.Prompt.Should().Be(topLevelPrompt);
                }
            }

            [Test]
            [Category.R.Session]
            public async Task InteractionContexts() {
                Task<IRSessionInteraction> interTask;
                using (var inter = await _session.BeginInteractionAsync()) {
                    inter.Contexts.IsBrowser().Should().BeFalse();

                    // Request a new interaction before finishing this one, so that request gets queued. 
                    interTask = _session.BeginInteractionAsync();

                    // Trigger a Browse> prompt, which will have different contexts.
                    await inter.RespondAsync("browser()\n");
                }

                // Check that task queued before the new prompt has the appropriate contexts for that prompt.
                (await interTask).Contexts.IsBrowser().Should().BeTrue();
            }
        }
    }
}
