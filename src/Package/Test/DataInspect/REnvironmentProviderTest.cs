// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Microsoft.VisualStudio.R.Package.DataInspect;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]   // required for tests using R Host 
    public class REnvironmentProviderTest : IAsyncLifetime {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public REnvironmentProviderTest(IServiceContainer services, TestMethodFixture testMethod) {
            _sessionProvider = new RSessionProvider(services);
            _session = _sessionProvider.GetOrCreate(testMethod.FileSystemSafeName);
        }

        public async Task InitializeAsync() {
            await _sessionProvider.TrySwitchBrokerAsync(nameof(REnvironmentProviderTest));
            await _session.StartHostAsync(new RHostStartupInfo (), new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        private static IREnvironment Environment(string name, REnvironmentKind kind) {
            var env = Substitute.For<IREnvironment>();
            env.Name.Returns(name);
            env.Kind.Returns(kind);
            return env;
        }

        [Test]
        [Category.Variable.Explorer]
        public Task MinimalEnvironments() {
            return Environments("browser()",
                Environment(".GlobalEnv", REnvironmentKind.Global),
                Environment("package:base", REnvironmentKind.Package));
        }

        [Test]
        [Category.Variable.Explorer]
        public Task PackageEnvironments() {
            return Environments("library(methods); library(utils); browser()",
                Environment(".GlobalEnv", REnvironmentKind.Global),
                Environment("package:utils", REnvironmentKind.Package),
                Environment("package:methods", REnvironmentKind.Package),
                Environment("package:base", REnvironmentKind.Package));
        }

        [Test]
        [Category.Variable.Explorer]
        public Task FunctionEnvironments() {
            return Environments("foo <- function(tag = 'foo') browser(); bar <- function(tag = 'bar') foo(); bar(); ",
                Environment("foo()", REnvironmentKind.Function),
                Environment("bar()", REnvironmentKind.Function),
                null,
                Environment(".GlobalEnv", REnvironmentKind.Global),
                Environment("package:base", REnvironmentKind.Package));
        }

        [Test]
        [Category.Variable.Explorer]
        public Task AttachedEnvironments() {
            return Environments("attach(list(x = 1)); attach(list(y = 2)); browser()",
                Environment(".GlobalEnv", REnvironmentKind.Global),
                Environment("list(y = 2)", REnvironmentKind.Unknown),
                Environment("list(x = 1)", REnvironmentKind.Unknown),
                Environment("package:base", REnvironmentKind.Package));
        }

        private async Task Environments(string script, params IREnvironment[] expectedEnvs) {
            // Detach all packages that can be detached before doing anything else. The only two that
            // cannot be detached are .GlobalEnv, which is the first in the list, and package:base,
            // which is the last. So, just keep detaching the 2nd item until the list only has 2 left.
            await _session.ExecuteAsync("while (length(search()) > 2) detach(2)");

            // Wait for prompt to appear.
            using (await _session.BeginInteractionAsync()) { }

            var envProvider = new REnvironmentProvider(_session, UIThreadHelper.Instance.MainThread);
            var envTcs = new TaskCompletionSource<IREnvironment[]>();
            envProvider.Environments.CollectionChanged += (sender, args) => {
                envTcs.TrySetResult(envProvider.Environments.ToArray());
            };

            using (var inter = await _session.BeginInteractionAsync()) {
                inter.RespondAsync(script + "\n").DoNotWait();
            }
            
            // Wait until we hit the Browse> prompt.
            using (var inter = await _session.BeginInteractionAsync()) {
                inter.Contexts.IsBrowser().Should().BeTrue();
            }

            var actualEnvs = await envTcs.Task;

            actualEnvs.ShouldAllBeEquivalentTo(expectedEnvs, options => options
                .Including(env => env.Name)
                .Including(env => env.Kind)
                .WithStrictOrdering());

            // Validating EnvironmentExpression:
            // For environments that are on the search list, we can validate it by retrieving environment by name,
            // and verifying that it is indeed the same. format() generates comparable string values - it returns strings
            // that are unique for any given environment (using its name if it has one, and its memory address otherwise).
            // For all other environments, we validate by looking at a variable named 'tag' in that environment, and
            // comparing its value to the name of the function extracted from the call (i.e. environment name).
            var expectedObjects = new List<string>();
            var actualObjects = new List<string>();

            foreach (var env in actualEnvs) {
                if (env == null) {
                    expectedObjects.Add(null);
                    actualObjects.Add(null);
                } else if (env.Kind == REnvironmentKind.Function) {
                    int tagEnd = env.Name.IndexOf("(");
                    tagEnd.Should().BePositive();
                    string tag = env.Name.Substring(0, tagEnd);

                    expectedObjects.Add(tag);
                    actualObjects.Add(await _session.EvaluateAsync<string>(
                        $"({env.EnvironmentExpression})$tag",
                        REvaluationKind.Normal));
                } else {
                    expectedObjects.Add(await _session.EvaluateAsync<string>(
                        $"format(as.environment({env.Name.ToRStringLiteral()}))",
                        REvaluationKind.Normal));
                    actualObjects.Add(await _session.EvaluateAsync<string>(
                        $"format({env.EnvironmentExpression})",
                        REvaluationKind.Normal));
                }
            }

            actualObjects.Should().Equal(expectedObjects);
        }
    }
}
