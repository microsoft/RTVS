// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [AttributeUsage(AttributeTargets.Assembly)]
    [TestFrameworkDiscoverer("Microsoft.UnitTests.Core.XUnit.TestFrameworkTypeDiscoverer", "Microsoft.UnitTests.Core")]
    public sealed class TestFrameworkOverrideAttribute : Attribute, ITestFrameworkAttribute {
    }
}