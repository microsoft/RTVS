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
        private ManualResetEvent _environmentChanged = new ManualResetEvent(false);
        private REnvironmentCollection _environments;

        [Test]
        [Category.Variable.Explorer]
        public async Task GetREnvironmentTest() {
            await RunEnvironmentTestAsync("1", () => {
                _environments.Count.Should().BeGreaterOrEqualTo(2);
                _environments[0].Name.Should().BeEquivalentTo(".GlobalEnv");
                _environments[_environments.Count - 1].Name.Should().BeEquivalentTo("package:base");
            });
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task GetREnvironmentFuncionTest() {
            await RunEnvironmentTestAsync("foo<-function(x){browser();};foo(1);", () => {
                _environments.Count.Should().BeGreaterOrEqualTo(3);
                _environments[0].Name.Should().BeEquivalentTo("foo");
                _environments[0].FrameIndex.HasValue.Should().BeTrue();
                _environments[0].FrameIndex.Should().Be(1);
                _environments[1].Name.Should().BeEquivalentTo(".GlobalEnv");
                _environments[_environments.Count - 1].Name.Should().BeEquivalentTo("package:base");
            });
        }

        private  async Task RunEnvironmentTestAsync(string expression, Action assert) {
            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            _environmentChanged.Reset();
            _environments = null;

            using (var rHostScript = new RHostScript(sessionProvider, null)) {
                using (var environmentProvider = new REnvironmentProvider(rHostScript.Session)) {
                    environmentProvider.EnvironmentChanged += EnvironmentProvider_EnvironmentChanged;

                    using (var interaction = await rHostScript.Session.BeginInteractionAsync(false)) {
                        await interaction.RespondAsync(expression + Environment.NewLine);
                    }

                    bool timedout;
                    if (System.Diagnostics.Debugger.IsAttached) {
                        timedout = _environmentChanged.WaitOne();
                    } else {
                        timedout = _environmentChanged.WaitOne(TimeSpan.FromSeconds(10));
                    }
                    timedout.Should().BeTrue();

                    environmentProvider.EnvironmentChanged -= EnvironmentProvider_EnvironmentChanged;

                    assert();
                }
            }
        }

        private void EnvironmentProvider_EnvironmentChanged(object sender, REnvironmentChangedEventArgs e) {
            _environments = e.Environments;
            _environmentChanged.Set();
        }
    }
}
