// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    public class TestFrameworkTypeDiscoverer : ITestFrameworkTypeDiscoverer {
        public Type GetTestFrameworkType(IAttributeInfo attribute) => typeof(TestFramework);
    }
}