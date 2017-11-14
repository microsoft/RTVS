// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal sealed class ClassRunner : XunitTestClassRunner {
        private readonly Dictionary<int, Type> _methodFixtureTypes = new Dictionary<int, Type>();
        private readonly IReadOnlyDictionary<Type, object> _assemblyFixtureMappings;
        private readonly XunitTestEnvironment _testEnvironment;

        public ClassRunner(ITestClass testClass, IReflectionTypeInfo typeInfo, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, IDictionary<Type, object> collectionFixtureMappings, IReadOnlyDictionary<Type, object> assemblyFixtureMappings, XunitTestEnvironment testEnvironment)
            : base(testClass, typeInfo, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings) {
            _assemblyFixtureMappings = assemblyFixtureMappings;
            _testEnvironment = testEnvironment;
        }

        protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments) 
            => new TestMethodRunner(testMethod, Class, method, testCases, DiagnosticMessageSink, MessageBus, new ExceptionAggregator(Aggregator), CancellationTokenSource, constructorArguments, _methodFixtureTypes, _assemblyFixtureMappings, _testEnvironment).RunAsync();

        protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object argumentValue) 
            => TryGetArgumentDummy(parameter, index, out argumentValue)
               || base.TryGetConstructorArgument(constructor, index, parameter, out argumentValue) 
               || _assemblyFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue);

        private bool TryGetArgumentDummy(ParameterInfo parameter, int index, out object argumentValue) {
            argumentValue = null; // real value will be calculated later, cause it should be separate instance for every test case 
            var type = parameter.ParameterType;
            if (type.IsValueType) {
                return false;
            }

            if (_assemblyFixtureMappings.TryGetValue(type, out var assemblyFixture) && assemblyFixture is IMethodFixtureFactory) {
                _methodFixtureTypes[index] = type;
                return true;
            }

            if (typeof(IMethodFixture).IsAssignableFrom(parameter.ParameterType)) {
                _methodFixtureTypes[index] = type;
                return true;
            }

            return false;
        }
    }
}