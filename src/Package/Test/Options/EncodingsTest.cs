// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Xunit;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Test.Options {
    [ExcludeFromCodeCoverage]
    [Category.Repl]
    [Collection(CollectionNames.NonParallel)]
    public class EncodingsTest: IDisposable {
        private readonly IRHostBrokerConnector _brokerConnector;
        private readonly IRSessionProvider _sessionProvider;

        public EncodingsTest() {
            _brokerConnector = new RHostBrokerConnector();
            _brokerConnector.SwitchToLocalBroker(nameof(EncodingsTest));
            _sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
        }

        public void Dispose() {
            _brokerConnector.Dispose();
        }

        [Test]
        public void ValidateEncodings() {
            var etc = new EncodingTypeConverter();
            var codePages = etc.GetStandardValues();
            using(var script = new VsRHostScript(_sessionProvider, _brokerConnector)) {
                foreach (var cp in codePages) {
                    if ((int)cp > 0) {
                        var completed = Task.Run(async () => {
                            var expression = Invariant($"Sys.setlocale('LC_CTYPE', '.{cp}')\n");
                            using (var inter = await script.Session.BeginInteractionAsync()) {
                                await inter.RespondAsync(expression);
                            }
                        }).Wait(5000);
                        completed.Should().BeTrue(because: "Sys.setlocale() didn't complete within 5000 ms");

                        string s = null;
                        completed = Task.Run(async () => {
                            var res = await script.Session.EvaluateAsync("Sys.getlocale()", REvaluationKind.Normal);
                            s = res.Result.ToString();
                        }).Wait(5000);

                        completed.Should().BeTrue(because: "Sys.getlocale() didn't complete within 5000 ms");
                        s.Should().NotBeNull().And.Contain(cp.ToString());
                    }
                }
            }
        }
    }
}
