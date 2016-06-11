// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Xunit;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Test.Options {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class EncodingsTest {
        [Test]
        public async Task ValidateEncodings() {
            var etc = new EncodingTypeConverter();
            var codePages = etc.GetStandardValues();
            using(var script = new VsRHostScript()) {
                foreach (var cp in codePages) {
                    if ((int)cp > 0) {
                        var completed = Task.Run(async () => {
                            var expression = Invariant($"Sys.setlocale('LC_CTYPE', '.{cp}')\n");
                            using (var inter = await script.Session.BeginInteractionAsync()) {
                                await inter.RespondAsync(expression);
                            }
                        }).Wait(2000);
                        completed.Should().BeTrue();

                        string s = null;
                        completed = Task.Run(async () => {
                            using (var e = await script.Session.BeginEvaluationAsync()) {
                                var res = await e.EvaluateAsync("Sys.getlocale()", REvaluationKind.Normal);
                                s = res.Result.ToString();
                            }
                        }).Wait(2000);

                        completed.Should().BeTrue();
                        s.Should().NotBeNull().And.Contain(cp.ToString());
                    }
                }
            }
        }
    }
}
