// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    /// <summary>
    /// Allows to inject logic into tests. 
    /// New instance of derived class is created for each test case.
    /// Constructor shouldn't contain any initialization logic, which should all be inside <see cref="InitializeAsync"/> method
    /// </summary>
    public interface IMethodFixture {
        /// <summary>
        /// Initializes method fixture.
        /// </summary>
        /// <param name="testInput">Test method input data (constructor and method arguments, fixtures, etc.)</param>
        /// <param name="messageBus"></param>
        Task InitializeAsync(ITestInput testInput, IMessageBus messageBus);

        Task DisposeAsync(RunSummary result, IMessageBus messageBus);
    }
}