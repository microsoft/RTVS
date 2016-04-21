// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Host.Client.Session;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Host.Client.Test.Session {
    public partial class RSessionTest {
        public class Output : IAsyncLifetime {
            private readonly MethodInfo _testMethod;
            private readonly RSession _session;

            public Output(TestMethodFixture testMethod) {
                _testMethod = testMethod.MethodInfo;
                _session = new RSession(0, () => { });
            }

            public async Task InitializeAsync() {
                await _session.StartHostAsync(new RHostStartupInfo {
                    Name = _testMethod.Name,
                    RBasePath = RUtilities.FindExistingRBasePath()
                }, null, 50000);
            }

            public async Task DisposeAsync() {
                await _session.StopHostAsync();
                _session.Dispose();
            }

            [Test]
            [Category.R.Session]
            public async Task UnicodeOutput() {
                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("Sys.setlocale('LC_CTYPE', 'Japanese_Japan.932')\n");
                }

                var output = new StringBuilder();
                _session.Output += (sender, e) => output.Append(e.Message);

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("'日本語'\n");
                }

                output.ToString().Should().Be("[1] \"日本語\"\n");
            }
        }
    }
}
