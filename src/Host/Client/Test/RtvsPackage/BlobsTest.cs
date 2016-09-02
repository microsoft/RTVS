// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.Interpreters;
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
            await _sessionProvider.TrySwitchBroker(nameof(BlobsTest));
            await _session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        [CompositeTest]
        [Category.R.RtvsPackage]
        [InlineData(new byte[] { 0, 1, 2, 3, 4 })]
        public async Task RawResult(byte[] data) {
            string expr = $"as.raw(c({string.Join(", ", data)}))";
            var res = await _session.EvaluateAsync(expr, REvaluationKind.RawResult);

            res.Result.Should().BeNull();
            res.RawResult.Should().Equal(data);

            var bytes = await _session.EvaluateAsync<byte[]>(expr, REvaluationKind.Normal);
            bytes.Should().Equal(data);
        }

        [Test]
        [Category.R.RtvsPackage]
        public async Task RawResultNull() {
            var res = await _session.EvaluateAsync("NULL", REvaluationKind.RawResult);
            res.Result.Should().BeNull();
            res.RawResult.Should().Equal(new byte[0]);

            var bytes = await _session.EvaluateAsync<byte[]>("NULL", REvaluationKind.Normal);
            bytes.Should().Equal(new byte[0]);
        }

        [Test]
        public async Task CreateGetDestroyBlobs() {
            byte[] data1 = new byte[] { 0, 1, 2, 3, 4, 5 };
            byte[] data2 = new byte[] { 10, 11, 12, 13, 14, 15 };
            byte[] data3 = new byte[] { 20, 21, 22, 23, 24, 25 };
            var dataSet = new byte[][] { data1, data2, data3 };

            var blob1 = await _session.CreateBlobAsync(data1);
            var blob2 = await _session.CreateBlobAsync(data2);
            var blob3 = await _session.CreateBlobAsync(data3);

            blob1.Should().BeGreaterThan(0);
            blob2.Should().BeGreaterThan(0);
            blob3.Should().BeGreaterThan(0);

            blob1.Should().NotBe(blob2);
            blob2.Should().NotBe(blob3);
            blob3.Should().NotBe(blob1);
            var blobIds = new ulong[] { blob1, blob2, blob3 };

            for (int i = 0; i < blobIds.Length; ++i) {
                var blob = await _session.GetBlobAsync(blobIds[i]);
                blob.Should().Equal(dataSet[i]);
            }

            await _session.DestroyBlobsAsync(blobIds);
        }

        [Test]
        public async Task ZeroSizedBlob() {
            byte[] data = new byte[] { };
            var blobId = await _session.CreateBlobAsync(data);
            blobId.Should().BeGreaterThan(0);

            var res = await _session.GetBlobAsync(blobId);
            res.Should().Equal(new byte[0]);

            await _session.DestroyBlobsAsync(new[] { blobId });
        }

        [Test]
        public async Task RCreateGetDestroyBlobs() {
            using (var eval = await _session.BeginEvaluationAsync()) {
                var createResult = await eval.EvaluateAsync("rtvs:::create_blob(as.raw(1:10))", REvaluationKind.Normal);
                createResult.Result.Should().NotBeNull();

                var blobId = ((JValue)createResult.Result).Value<ulong>();
                var actualData = await eval.EvaluateAsync<byte[]>($"rtvs:::get_blob({blobId})", REvaluationKind.Normal);

                byte[] expectedData = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                actualData.Should().Equal(expectedData);
                
                await eval.ExecuteAsync($"rtvs:::destroy_blob({blobId})");
            }
        }
    }
}
