// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Support.Test.Functions {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    public class FunctionInfoTest : IAsyncLifetime {
        private readonly IExportProvider _exportProvider;
        private readonly IPackageIndex _packageIndex;
        private readonly IFunctionIndex _functionIndex;
        private readonly IRInteractiveWorkflow _workflow;

        public FunctionInfoTest(IExportProvider exportProvider) {
            _exportProvider = exportProvider;
            _workflow = UIThreadHelper.Instance.Invoke(() => _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate());
            _packageIndex = _exportProvider.GetExportedValue<IPackageIndex>();
            _functionIndex = _exportProvider.GetExportedValue<IFunctionIndex>();
        }

        public async Task InitializeAsync() {
            await _workflow.RSessions.TrySwitchBrokerAsync(GetType().Name);
            await _packageIndex.BuildIndexAsync();
        }

        public Task DisposeAsync() => _packageIndex.DisposeAsync(_exportProvider);

        [CompositeTest]
        [InlineData("abs")]
        [InlineData("zzz")]
        public async Task FunctionInfoTest1(string name) {
            var functionInfo = await PackageIndexUtility.GetFunctionInfoAsync(_functionIndex, "abs");

            functionInfo.Should().NotBeNull();
            functionInfo.Name.Should().Be("abs");
            functionInfo.Description.Should().NotBeEmpty();
            functionInfo.Signatures.Should().ContainSingle()
                .Which.Arguments.Should().ContainSingle();

            List<int> locusPoints = new List<int>();
            functionInfo.Signatures[0].GetSignatureString(name, locusPoints).Should().Be(name + "(x)");
            locusPoints.Should().Equal(4, 5);
        }

        [Test]
        public async Task FunctionInfoTest2() {
            var functionInfo = await PackageIndexUtility.GetFunctionInfoAsync(_functionIndex, "eval");

            functionInfo.Should().NotBeNull();
            functionInfo.Name.Should().Be("eval");
            functionInfo.Description.Should().NotBeEmpty();
            functionInfo.Signatures.Should().ContainSingle().Which.Arguments.Should().HaveCount(4);

            List<int> locusPoints = new List<int>();
            string signature = functionInfo.Signatures[0].GetSignatureString("eval", locusPoints);
            signature.Should().Be("eval(expr, envir = parent.frame(), enclos = if(is.list(envir) || is.pairlist(envir)) parent.frame() else baseenv(), n)");
            locusPoints.Should().Equal(5, 11, 35, 116, 117);
        }
    }
}
