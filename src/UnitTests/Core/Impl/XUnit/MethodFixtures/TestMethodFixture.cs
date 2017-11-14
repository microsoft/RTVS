// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit.MethodFixtures {
    public class TestMethodFixture : IMethodFixture {
        public MethodInfo MethodInfo { get; private set; }
        public string DisplayName { get; private set; }
        public string FileSystemSafeName { get; private set; }

        public Task InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            DisplayName = testInput.DisplayName;
            MethodInfo = testInput.TestMethod;
            FileSystemSafeName = testInput.FileSytemSafeName;
            return Task.CompletedTask;
        }

        public Task DisposeAsync(RunSummary result, IMessageBus messageBus) => Task.CompletedTask;
    }
}