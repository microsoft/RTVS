// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.UnitTests.Core.XUnit {
    /// <summary>
    /// Allows assembly fixtures to create method fixtures for every single test run
    /// Method fixtures are disposed at the end of the test run
    /// 
    /// IMethodFixtureFactory implementation should have method T Create(...), which can have other IMethodFixture isntances as arguments
    /// </summary>
    public interface IMethodFixtureFactory {}
}