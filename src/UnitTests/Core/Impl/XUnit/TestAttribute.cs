// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    [ExcludeFromCodeCoverage]
    [XunitTestCaseDiscoverer("Microsoft.UnitTests.Core.XUnit.TestDiscoverer", "Microsoft.UnitTests.Core")]
    [TraitDiscoverer("Microsoft.UnitTests.Core.XUnit.UnitTestTraitDiscoverer", "Microsoft.UnitTests.Core")]
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : FactAttribute, ITraitAttribute
    {
        public TestAttribute(ThreadType threadType = ThreadType.Default)
        {
            ThreadType = threadType;
        }

        public ThreadType ThreadType { get; set; }
    }
}