// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    internal sealed class ClassRunner : XunitTestClassRunner {
        private static readonly IDictionary<Type, object> Dummies = new Dictionary<Type, object>();

        internal static readonly TestMethodFixture TestMethodFixtureDummy = new TestMethodFixture();
        private readonly IReadOnlyDictionary<Type, object> _assemblyFixtureMappings;

        public ClassRunner(ITestClass testClass, IReflectionTypeInfo typeInfo, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, IDictionary<Type, object> collectionFixtureMappings, IReadOnlyDictionary<Type, object> assemblyFixtureMappings)
            : base(testClass, typeInfo, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings) {
            _assemblyFixtureMappings = assemblyFixtureMappings;
        }

        protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments) {
            return new TestMethodRunner(testMethod, Class, method, testCases, DiagnosticMessageSink, MessageBus, new ExceptionAggregator(Aggregator), CancellationTokenSource, constructorArguments, _assemblyFixtureMappings).RunAsync();
        }

        protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object argumentValue) {
            if (typeof(IMethodFixture).IsAssignableFrom(parameter.ParameterType)) {
                argumentValue = GetArgumentDummy(parameter);
                return true;
            }

            return base.TryGetConstructorArgument(constructor, index, parameter, out argumentValue) 
                || _assemblyFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue);
        }

        private object GetArgumentDummy(ParameterInfo parameter) {
            object argumentValue;
            if (Dummies.TryGetValue(parameter.ParameterType, out argumentValue)) {
                return argumentValue;
            }

            object assemblyFixture;
            IMethodFixtureFactory<IMethodFixture> methodFixtureFactory;
            if (_assemblyFixtureMappings.TryGetValue(parameter.ParameterType, out assemblyFixture) &&
                (methodFixtureFactory = (IMethodFixtureFactory<IMethodFixture>) assemblyFixture) != null) {

                argumentValue = methodFixtureFactory.Dummy;
            } else {
                argumentValue = Activator.CreateInstance(parameter.ParameterType);
            }

            Dummies[parameter.ParameterType] = argumentValue;
            return argumentValue;
        }
    }
}