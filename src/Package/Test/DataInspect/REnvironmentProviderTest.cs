// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.Shell;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]   // required for tests using R Host 
    public class REnvironmentProviderTest {
        [Test]
        [Category.Variable.Explorer]
        public async Task GetREnvironmentTest() {
            await RunEnvironmentTestAsync("1", (environments) => {
                environments.Count.Should().BeGreaterOrEqualTo(2);
                environments[0].Name.Should().Be(".GlobalEnv");
                environments[environments.Count - 1].Name.Should().Be("package:base");
            });
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task GetREnvironmentFunctionTest() {
            await RunEnvironmentTestAsync("foo<-function(x){browser();};foo(1);", (environments) => {
                environments.Count.Should().BeGreaterOrEqualTo(3);
                environments[0].Name.Should().Be("foo");
                environments[0].FrameIndex.HasValue.Should().BeTrue();
                environments[0].FrameIndex.Should().Be(1);
                environments[1].Name.Should().Be(".GlobalEnv");
                environments[environments.Count - 1].Name.Should().Be("package:base");
            });
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task GetREnvironmentFunction1Test() {
            await RunEnvironmentTestAsync("foo<-function(x){browser();};bar<-function(x){foo(x+1);};bar(1);", (environments) => {
                environments.Count.Should().BeGreaterOrEqualTo(3);
                environments[0].Name.Should().Be("foo");
                environments[0].FrameIndex.HasValue.Should().BeTrue();
                environments[0].FrameIndex.Should().Be(2);
                environments[1].Name.Should().Be(".GlobalEnv");
                environments[environments.Count - 1].Name.Should().Be("package:base");
            });
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task GetREnvironmentFunction2Test() {
            await RunEnvironmentTestAsync("bar<-function(x){foo<-function(x){browser();};foo(x+1);};bar(1);", (environments) => {
                environments.Count.Should().BeGreaterOrEqualTo(4);
                environments[0].Name.Should().Be("foo");
                environments[0].FrameIndex.HasValue.Should().BeTrue();
                environments[0].FrameIndex.Should().Be(2);
                environments[1].Name.Should().Be("bar");
                environments[1].FrameIndex.HasValue.Should().BeTrue();
                environments[1].FrameIndex.Should().Be(1);
                environments[2].Name.Should().Be(".GlobalEnv");
                environments[environments.Count - 1].Name.Should().Be("package:base");
            });
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task GetREnvironmentFunction3Test() {
            await RunEnvironmentTestAsync("bar<-function(x){foo<-function(y){browser();x+y;};return(foo);};foo<-bar(1);print(foo(2));", (environments) => {
                environments.Count.Should().BeGreaterOrEqualTo(3);
                environments[0].Name.Should().Be("foo");
                environments[0].FrameIndex.HasValue.Should().BeTrue();
                environments[0].FrameIndex.Should().Be(2);
                environments[1].Name.Should().Be(".GlobalEnv");
                environments[environments.Count - 1].Name.Should().Be("package:base");
            });
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task GetREnvironmentFunction4Test() {
            await RunEnvironmentTestAsync("x<-new.env();attach(x);", (environments) => {
                environments.Count.Should().BeGreaterOrEqualTo(3);
                environments[0].Name.Should().Be(".GlobalEnv");
                environments[1].Name.Should().Be("x");
                environments[0].FrameIndex.HasValue.Should().BeFalse();
                environments[environments.Count - 1].Name.Should().Be("package:base");
            });
        }

        private  async Task RunEnvironmentTestAsync(string expression, Action<IReadOnlyList<REnvironment>> assert) {
            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var environmentChanged = new CountdownEvent(3);
            IReadOnlyList<REnvironment> environments = null;

            using (var rHostScript = new RHostScript(sessionProvider, null)) {
                using (var environmentProvider = new REnvironmentProvider(rHostScript.Session)) {
                    environmentProvider.EnvironmentChanged += (sender, e) => {
                        environments = e.Environments;
                        environmentChanged.Signal();
                    };
                    using (var interaction = await rHostScript.Session.BeginInteractionAsync(false)) {
                        await interaction.RespondAsync(expression + Environment.NewLine);
                    }

                    bool timedout = true;
                    if (System.Diagnostics.Debugger.IsAttached) {
                        environmentChanged.Wait();
                    } else {
                        timedout = environmentChanged.Wait(TimeSpan.FromSeconds(10));
                    }
                    timedout.Should().BeTrue();

                    assert(environments);
                }
            }
        }
    }
}
