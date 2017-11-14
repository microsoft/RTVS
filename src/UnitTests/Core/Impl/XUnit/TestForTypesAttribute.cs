// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [XunitTestCaseDiscoverer("Microsoft.UnitTests.Core.XUnit.TestForTypesDiscoverer", "Microsoft.UnitTests.Core")]
    [AttributeUsage(AttributeTargets.Method)]
    public class TestForTypesAttribute : FactAttribute, ITraitAttribute {
        public TestForTypesAttribute(params Type[] types) {
            Types = types;
        }

        public ThreadType ThreadType { get; set; }
        public Type[] Types { get; set; }
    }
}