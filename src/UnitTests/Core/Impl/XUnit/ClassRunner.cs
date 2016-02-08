using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    internal sealed class ClassRunner : XunitTestClassRunner {
        private static readonly TestMethodInfoFixture TestMethodInfoFixtureDummy = new TestMethodInfoFixture();
        private readonly IReadOnlyDictionary<Type, object> _assemblyFixtureMappings;

        public ClassRunner(ITestClass testClass, IReflectionTypeInfo typeInfo, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, IDictionary<Type, object> collectionFixtureMappings, IReadOnlyDictionary<Type, object> assemblyFixtureMappings)
            : base(testClass, typeInfo, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings) {
            _assemblyFixtureMappings = assemblyFixtureMappings;
        }

        protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object argumentValue) {
            if (parameter.ParameterType == typeof (TestMethodInfoFixture)) {
                // We want to provide unique instance for every test, so for now just add a default dummy that will be replaced in RunTestMethodAsync with real value
                argumentValue = TestMethodInfoFixtureDummy;
                return true;
            }

            return base.TryGetConstructorArgument(constructor, index, parameter, out argumentValue) || _assemblyFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue);
        }

        protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments) {
            var testInformationFixtureIndex = Array.IndexOf(constructorArguments, TestMethodInfoFixtureDummy);
            if (testInformationFixtureIndex == -1) {
                return base.RunTestMethodAsync(testMethod, method, testCases, constructorArguments);
            }

            var constructorArgumentsCopy = new object[constructorArguments.Length];
            Array.Copy(constructorArguments, constructorArgumentsCopy, constructorArguments.Length);
            constructorArgumentsCopy[testInformationFixtureIndex] = new TestMethodInfoFixture(testMethod.Method.ToRuntimeMethod());

            return base.RunTestMethodAsync(testMethod, method, testCases, constructorArgumentsCopy);
        }
    }
}