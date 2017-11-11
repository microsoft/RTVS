// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal sealed class AssemblyRunner : XunitTestAssemblyRunner {
        private readonly XunitTestEnvironment _testEnvironment;
        private IReadOnlyDictionary<Type, object> _assemblyFixtureMappings;
        private IList<AssemblyLoaderAttribute> _assemblyLoaders;

        public AssemblyRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions, XunitTestEnvironment testEnvironment)
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions) {
            _testEnvironment = testEnvironment;
        }

        protected override async Task AfterTestAssemblyStartingAsync() {
            await base.AfterTestAssemblyStartingAsync();

            _assemblyLoaders = AssemblyLoaderAttribute.GetAssemblyLoaders(TestAssembly.Assembly);
            foreach (var assemblyLoader in _assemblyLoaders) {
                assemblyLoader.Initialize();
            }

            var assembly = TestAssembly.Assembly;
            var importedAssemblyFixtureTypes = assembly.GetCustomAttributes(typeof(AssemblyFixtureImportAttribute))
                .SelectMany(ai => ai.GetConstructorArguments())
                .OfType<Type[]>()
                .SelectMany(a => a);

            var assemblyFixtureTypes = assembly.GetTypes(false)
                .Where(t => t.GetCustomAttributes(typeof(AssemblyFixtureAttribute).AssemblyQualifiedName).Any())
                .Select(t => t.ToRuntimeType())
                .Concat(importedAssemblyFixtureTypes)
                .ToList();

            var fixtures = new Dictionary<Type, object>();

            foreach (var type in assemblyFixtureTypes) {
                await Aggregator.RunAsync(() => AddAssemblyFixtureAsync(fixtures, type));
            }

            _assemblyFixtureMappings = new ReadOnlyDictionary<Type, object>(fixtures);
        }

        protected override async Task BeforeTestAssemblyFinishedAsync() {
            foreach (var asyncLifetime in _assemblyFixtureMappings.Values.OfType<IAsyncLifetime>()) {
                await Aggregator.RunAsync(asyncLifetime.DisposeAsync);
            }

            foreach (var disposable in _assemblyFixtureMappings.Values.OfType<IDisposable>()) {
                Aggregator.Run(disposable.Dispose);
            }

            foreach (var assemblyLoader in _assemblyLoaders) {
                assemblyLoader.Dispose();
            }

            await base.BeforeTestAssemblyFinishedAsync();
        }

        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, CancellationTokenSource cancellationTokenSource) {
            return new CollectionRunner(testCollection, testCases, DiagnosticMessageSink, messageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource, _assemblyFixtureMappings, _testEnvironment).RunAsync();
        }

        private static async Task AddAssemblyFixtureAsync(IDictionary<Type, object> fixtures, Type fixtureType) {
            var fixture = Activator.CreateInstance(fixtureType);
            if (fixture is IAsyncLifetime asyncLifetime) {
                await asyncLifetime.InitializeAsync();
            }

            if (typeof(ITestMainThreadFixture).IsAssignableFrom(fixtureType)) {
                fixtures[typeof(ITestMainThreadFixture)] = fixture;
            }

            fixtures[fixtureType] = fixture;
            var methodFixtureTypes = MethodFixtureProvider.GetFactoryMethods(fixtureType).Select(mi => mi.ReturnType);
            foreach (var type in methodFixtureTypes) {
                fixtures[type] = fixture;
            }
        }

        private static bool IsGenericType(Type t) =>  t.GetTypeInfo().IsGenericType;
    }
}
