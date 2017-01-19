// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit.MethodFixtures {
    public abstract class MethodFixtureBase : IMethodFixture {
        public static Task<RunSummary> DefaultInitializeResult { get; } = new Task<RunSummary>(() => null);
        public static Task<Task<RunSummary>> DefaultInitializeTask { get; } = Task.FromResult(DefaultInitializeResult);

        public virtual Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) => DefaultInitializeTask;
        public virtual Task DisposeAsync(RunSummary result, IMessageBus messageBus) => Task.CompletedTask;
    }
}
