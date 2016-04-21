// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Host.Client.Session;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Host.Client.Test.Session {
    public partial class RSessionTest {
        public class InteractionEvaluation : IAsyncLifetime {
            private readonly MethodInfo _testMethod;
            private readonly RSession _session;

            public InteractionEvaluation(TestMethodFixture testMethod) {
                _testMethod = testMethod.MethodInfo;
                _session = new RSession(0, () => {});
            }

            public async Task InitializeAsync() {
                await _session.StartHostAsync(new RHostStartupInfo {
                    Name = _testMethod.Name,
                    RBasePath = RUtilities.FindExistingRBasePath()
                }, null, 50000);
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

            [Test(Skip="https://github.com/Microsoft/RTVS/issues/1193")]
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
        }
    }
}
