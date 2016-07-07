// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Host.Client.Install;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.R.Host.Client.Test.RtvsPackage {
    [ExcludeFromCodeCoverage]
    public class BlobsTest : IAsyncLifetime {
        private readonly MethodInfo _testMethod;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public BlobsTest(TestMethodFixture testMethod) {
            _testMethod = testMethod.MethodInfo;
            _sessionProvider = new RSessionProvider();
            _session = _sessionProvider.GetOrCreate(Guid.NewGuid());
        }

        public async Task InitializeAsync() {
            await _session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name,
                RBasePath = new RInstallation().GetRInstallPath()
            }, new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        [CompositeTest]
        [Category.R.RtvsPackage]
        [InlineData("charToRaw('0123456789')", "null", new int[] { 10 })]
        public async Task RawTest(string expr, string json, int[] sizes) {
            using (var eval = await _session.BeginEvaluationAsync()) {
                var res = await eval.EvaluateAsync(expr, REvaluationKind.Raw);
                var actualJson = JsonConvert.SerializeObject(res.Result).ToUnicodeQuotes();
                actualJson.Should().Be(json);
                res.Raw.Should().NotBeNull();
                res.Raw.Should().Equal(sizes, (a, e) => a.Length == e);
            }
        }

        [CompositeTest]
        [Category.R.RtvsPackage]
        [InlineData("NULL", "null")]
        public async Task NullTest(string expr, string json) {
            using (var eval = await _session.BeginEvaluationAsync()) {
                var res = await eval.EvaluateAsync(expr, REvaluationKind.Raw);
                var actualJson = JsonConvert.SerializeObject(res.Result).ToUnicodeQuotes();
                actualJson.Should().Be(json);
                res.Raw.Should().BeNull();
            }
        }
    }
}
