// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    /// <summary>
    /// Logic of this class requires change of the test method arguments, that cannot be done from decorator.
    /// </summary>
    internal class TestForTypesTestCase : TestCase {
        public TestForTypesTestCase() { }

        public TestForTypesTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod, TestParameters parameters, Type testMethodArgumentType)
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod, parameters, new object[] { testMethodArgumentType }) {
        }

        protected override object[] GetTestMethodArguments() {
            var testMethodArgumentType = (Type)TestMethodArguments.First();
            var testMethodArgument = Activator.CreateInstance(testMethodArgumentType);
            return new[] { testMethodArgument };
        }
    }
}