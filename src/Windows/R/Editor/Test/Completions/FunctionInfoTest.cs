// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Functions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    public class FunctionInfoTest : IAsyncLifetime {
        private readonly IPackageIndex _packageIndex;
        private readonly IFunctionIndex _functionIndex;
        private readonly IRInteractiveWorkflow _workflow;

        public FunctionInfoTest(IServiceContainer services) {
            var shell = services.GetService<ICoreShell>();
            _workflow = UIThreadHelper.Instance.Invoke(() => shell.GetService<IRInteractiveWorkflowProvider>().GetOrCreate());
            _packageIndex = shell.GetService<IPackageIndex>();
            _functionIndex = shell.GetService<IFunctionIndex>();
        }

        public async Task InitializeAsync() {
            await _workflow.RSessions.TrySwitchBrokerAsync(GetType().Name);
            await _packageIndex.BuildIndexAsync();
        }

        public Task DisposeAsync() {
            _packageIndex.Dispose();
            return Task.CompletedTask;
        }

        [CompositeTest]
        [InlineData("abs")]
        [InlineData("zzz")]
        public async Task FunctionInfoTest1(string name) {
            var functionInfo = await _functionIndex.GetFunctionInfoAsync("abs");

            functionInfo.Should().NotBeNull();
            functionInfo.Name.Should().Be("abs");
            functionInfo.Description.Should().NotBeEmpty();
            functionInfo.Signatures.Should().ContainSingle()
                .Which.Arguments.Should().ContainSingle();

            var locusPoints = new List<int>();
            functionInfo.Signatures[0].GetSignatureString(name, locusPoints).Should().Be(name + "(x)");
            locusPoints.Should().Equal(4, 5);
        }

        [Test]
        public async Task FunctionInfoTest2() {
            var functionInfo = await _functionIndex.GetFunctionInfoAsync("eval");

            functionInfo.Should().NotBeNull();
            functionInfo.Name.Should().Be("eval");
            functionInfo.Description.Should().NotBeEmpty();
            functionInfo.Signatures.Should().ContainSingle().Which.Arguments.Should().HaveCount(3);

            var locusPoints = new List<int>();
            var signature = functionInfo.Signatures[0].GetSignatureString("eval", locusPoints);
            signature.Should().Be("eval(expr, envir = parent.frame(), enclos = if(is.list(envir) || is.pairlist(envir)) parent.frame() else baseenv())");
            locusPoints.Should().Equal(5, 11, 35, 114);
        }
    }
}
