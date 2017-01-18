// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit.MethodFixtures {
    [ExcludeFromCodeCoverage]
    public class TestMethodFixture : MethodFixtureBase {
        public MethodInfo MethodInfo { get; private set; }
        public string DisplayName { get; private set; }
        public string FileSystemSafeName { get; private set; }

        public override Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            DisplayName = testInput.DisplayName;
            MethodInfo = testInput.TestMethod;
            FileSystemSafeName = testInput.FileSytemSafeName;
            return base.InitializeAsync(testInput, messageBus);
        }
    }
}