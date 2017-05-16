// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Xunit;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Test.Options {
    [ExcludeFromCodeCoverage]
    [Category.Repl]
    [Collection(CollectionNames.NonParallel)]
    public class EncodingsTest : HostBasedInteractiveTest {
        public EncodingsTest(IServiceContainer services) : base(services) { }

        [Test]
        public async Task ValidateEncodings() {
            var etc = new EncodingTypeConverter();
            var codePages = etc.GetStandardValues();
            foreach (var cp in codePages) {
                if ((int)cp > 0) {
                    var expression = Invariant($"Sys.setlocale('LC_CTYPE', '.{cp}')\n");
                    using (var inter = await HostScript.Session.BeginInteractionAsync()) {
                        await inter.RespondAsync(expression);
                    }

                    var res = await HostScript.Session.EvaluateAsync("Sys.getlocale()", REvaluationKind.Normal);
                    var s = res.Result.ToString();

                    s.Should().NotBeNull().And.Contain(cp.ToString());
                }
            }
        }
    }
}
