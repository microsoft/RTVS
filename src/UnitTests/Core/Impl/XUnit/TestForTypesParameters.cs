// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Xunit.Abstractions;

namespace Microsoft.UnitTests.Core.XUnit {
    public class TestForTypesParameters : TestParameters {
        public TestForTypesParameters(ITestMethod testMethod, IAttributeInfo factAttribute) : base(testMethod, factAttribute) {
            Types = factAttribute.GetNamedArgument<Type[]>("Types");
        }

        public Type[] Types { get; set; }
    }
}