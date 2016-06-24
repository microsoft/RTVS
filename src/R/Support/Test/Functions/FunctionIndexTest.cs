// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Support.Test.Functions {
    [ExcludeFromCodeCoverage]
    public class FunctionIndexTest : IAsyncLifetime {
        private readonly IExportProvider _exportProvider;

        public FunctionIndexTest(RSupportMefCatalogFixture catalog) {
            _exportProvider = catalog.CreateExportProvider();
        }

        public Task InitializeAsync() {
            return FunctionIndexUtility.InitializeAsync();
        }

        public async Task DisposeAsync() {
            await FunctionIndexUtility.DisposeAsync(_exportProvider);
            _exportProvider.Dispose();
        }

        [Test]
        [Category.R.Signatures]
         public async Task FunctionInfoTest1() {
            var functionInfo = await FunctionIndexUtility.GetFunctionInfoAsync("abs");

            functionInfo.Should().NotBeNull();
            functionInfo.Name.Should().Be("abs");
            functionInfo.Description.Should().NotBeEmpty();
            functionInfo.Signatures.Should().ContainSingle()
                .Which.Arguments.Should().ContainSingle();

            List<int> locusPoints = new List<int>();
            functionInfo.Signatures[0].GetSignatureString(locusPoints).Should().Be("abs(x)");
            locusPoints.Should().Equal(4, 5);
        }

        [Test]
        [Category.R.Signatures]
        public async Task FunctionInfoTest2() {
            var functionInfo = await FunctionIndexUtility.GetFunctionInfoAsync("eval");

            functionInfo.Should().NotBeNull();
            functionInfo.Name.Should().Be("eval");
            functionInfo.Description.Should().NotBeEmpty();
            functionInfo.Signatures.Should().ContainSingle()
                .Which.Arguments.Should().HaveCount(4);

            List<int> locusPoints = new List<int>();
            string signature = functionInfo.Signatures[0].GetSignatureString(locusPoints);
            signature.Should().Be("eval(expr, envir = parent.frame(), enclos = if(is.list(envir) || is.pairlist(envir)) parent.frame() else baseenv(), n)");
            locusPoints.Should().Equal(5, 11, 35, 116, 117);
        }
    }
}
