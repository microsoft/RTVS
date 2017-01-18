// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Xunit;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Test.Options {
    [ExcludeFromCodeCoverage]
    [Category.Repl]
    [Collection(CollectionNames.NonParallel)]
    public class EncodingsTest {
        private readonly ICoreServices _coreServices;

        public EncodingsTest(CoreServicesFixture coreServices) {
            _coreServices = coreServices;
        }

        [Test]
        public async Task ValidateEncodings() {
            var etc = new EncodingTypeConverter();
            var codePages = etc.GetStandardValues();
            using (var sessionProvider = new RSessionProvider(_coreServices)) {
                await sessionProvider.TrySwitchBrokerAsync(nameof(ValidateEncodings));
                using (var script = new VsRHostScript(sessionProvider)) {
                    foreach (var cp in codePages) {
                        if ((int)cp > 0) {
                            var expression = Invariant($"Sys.setlocale('LC_CTYPE', '.{cp}')\n");
                            using (var inter = await script.Session.BeginInteractionAsync()) {
                                await inter.RespondAsync(expression);
                            }

                            var res = await script.Session.EvaluateAsync("Sys.getlocale()", REvaluationKind.Normal);
                            var s = res.Result.ToString();

                            s.Should().NotBeNull().And.Contain(cp.ToString());
                        }
                    }
                }
            }
        }
    }
}
