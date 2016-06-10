// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
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
        public void ValidateEncodings() {
            var etc = new EncodingTypeConverter();
            var codePages = etc.GetStandardValues();
            using(var script = new VsRHostScript()) {
                foreach (var cp in codePages) {
                    var expression = Invariant($"Sys.setlocale('LC_CTYPE', '.{cp}')");
                    Action a = async () => await script.Session.EvaluateAsync(expression, REvaluationKind.Mutating);
                    a.ShouldNotThrow();
                }
            }
        }
    }
}
