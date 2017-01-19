// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    public interface ITestInput {
        IXunitTestCase TestCase { get; }
        Type TestClass { get; }
        MethodInfo TestMethod { get; }
        string DisplayName { get; }
        string FileSytemSafeName { get; }
        IReadOnlyList<object> ConstructorArguments { get; }
        IReadOnlyList<object> TestMethodArguments { get; }
    }
}