// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.UnitTests.Core.UI;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit.MethodFixtures {
    public class ContainerHostMethodFixture : IMethodFixture {
        public Task InitializeAsync(ITestInput testInput, IMessageBus messageBus) => ContainerHost.Increment();

        public Task DisposeAsync(RunSummary result, IMessageBus messageBus) => ContainerHost.Decrement();

        public Task<IDisposable> AddToHost(UIElement element) => ContainerHost.AddContainer(element);
    }
}


