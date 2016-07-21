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
using Newtonsoft.Json.Linq;
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

        [Test]
        public async Task CreateGetDestroyBlobsTest() {
            byte[] data1 = new byte[] { 0, 1, 2, 3, 4, 5 };
            byte[] data2 = new byte[] { 10, 11, 12, 13, 14, 15 };
            byte[] data3 = new byte[] { 20, 21, 22, 23, 24, 25 };
            var dataSet = new byte[][] { data1, data2, data3 };

            var blob1 = await _session.SendBlobAsync(data1);
            var blob2 = await _session.SendBlobAsync(data2);
            var blob3 = await _session.SendBlobAsync(data3);

            blob1.Should().BeGreaterThan(0);
            blob2.Should().BeGreaterThan(0);
            blob3.Should().BeGreaterThan(0);

            blob1.Should().NotBe(blob2);
            blob2.Should().NotBe(blob3);
            blob3.Should().NotBe(blob1);
            var blobIds = new long[] { blob1, blob2, blob3 };

            var res = await _session.GetBlobAsync(blobIds);
            res.Count.Should().Be(3);

            for(int i = 0; i < res.Count; ++i) {
                IRBlobData blob = res[i];
                blob.Id.Should().Be(blobIds[i]);
                blob.Data.ShouldAllBeEquivalentTo(dataSet[i]);
            }

            await _session.DestroyBlobAsync(blobIds);
        }

        [Test]
        public async Task BadSendBlobsRequest() {
            byte[] data = new byte[] { };
            var blobId = await _session.SendBlobAsync(data);
            blobId.Should().BeGreaterThan(0);

            var blobIds = new long[] { blobId };

            var res = await _session.GetBlobAsync(blobIds);
            res.Count.Should().Be(0);
            await _session.DestroyBlobAsync(blobIds);
        }

        [Test]
        public async Task BadGetBlobsRequest() {
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5 };
            var blobId = await _session.SendBlobAsync(data);
            blobId.Should().BeGreaterThan(0);

            var res = await _session.GetBlobAsync(new long[] { blobId + 1 });
            res.Count.Should().Be(0);
            await _session.DestroyBlobAsync(new long[] { blobId });
        }

        [Test]
        public async Task RCreateGetDestroyBlobsTest() {
            using (var eval = await _session.BeginEvaluationAsync()) {
                var createResult = await eval.EvaluateAsync("rtvs:::create_blob(as.raw(1:10))", REvaluationKind.Normal);
                createResult.Result.Should().NotBeNull();
                long blobId = ((JValue)createResult.Result).Value<long>();
                var getResult = await eval.EvaluateAsync($"rtvs:::get_blob({blobId})", REvaluationKind.Raw);

                byte[] expectedData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                getResult.Raw.Count.Should().Be(1);
                getResult.Raw[0].Length.Should().Be(expectedData.Length);
                getResult.Raw[0].ShouldAllBeEquivalentTo(expectedData);
                
                var destroyResult = await eval.EvaluateAsync($"rtvs:::destroy_blob({blobId})", REvaluationKind.NoResult);
                destroyResult.Result.Should().BeNull();
                destroyResult.Raw.Should().BeNull();
            }
        }
    }
}
