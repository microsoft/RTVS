// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using System.Threading;
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
        /// 
        /// </summary>
        /// <param name="testCase"></param>
        /// <param name="methodInfo">Test method metadata</param>
        /// <param name="messageBus"></param>
        /// <returns>
        /// A task that represents the asynchronous initialization. The value of the task contains a task will be observed by test runner. 
        /// If this task returns before test case is compeleted, it's <see cref="T:Xunit.Sdk.RunSummary"/> will be used instead. This method should NEVER fail
        /// </returns>
        Task<Task<RunSummary>> InitializeAsync(IXunitTestCase testCase, MethodInfo methodInfo, IMessageBus messageBus);

        Task DisposeAsync();
    }
}