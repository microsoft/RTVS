// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
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
            _sessionProvider = new RSessionProvider(TestCoreServices.CreateReal());
            _session = _sessionProvider.GetOrCreate(Guid.NewGuid());
        }

        public async Task InitializeAsync() {
            await _sessionProvider.TrySwitchBrokerAsync(nameof(BlobsTest));
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

            using (DataTransferSession dts = new DataTransferSession(_session, null)) {
                var blob1 = await dts.SendBytesAsync(data1);
                var blob2 = await dts.SendBytesAsync(data2);
                var blob3 = await dts.SendBytesAsync(data3);

                blob1.Id.Should().BeGreaterThan(0);
                blob2.Id.Should().BeGreaterThan(0);
                blob3.Id.Should().BeGreaterThan(0);

                blob1.Id.Should().NotBe(blob2.Id);
                blob2.Id.Should().NotBe(blob3.Id);
                blob3.Id.Should().NotBe(blob1.Id);

                var blobIds = new IRBlobInfo[] { blob1, blob2, blob3 };

                for (int i = 0; i < blobIds.Length; ++i) {
                    var blob = await dts.FetchBytesAsync(blobIds[i], false);
                    blob.Should().Equal(dataSet[i]);
                }
            }
        }
        
        [Test]
        public async Task CreateCopyToDestroyBlob() {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms)) {
                // write ~7MB of data
                for (int i = 0; i < 1000000; ++i) {
                    writer.Write((long)i);
                }
                writer.Flush();
                await ms.FlushAsync();
                ms.Position = 0;

                IRBlobInfo blob = null;
                using (RBlobStream blobStream = await RBlobStream.CreateAsync(_session)) {
                    await ms.CopyToAsync(blobStream);
                    blob = blobStream.GetBlobInfo();
                }

                using (RBlobStream blobStream = await RBlobStream.OpenAsync(blob, _session))
                using (MemoryStream ms2 = new MemoryStream()) {
                    await blobStream.CopyToAsync(ms2, 1024 * 1024);
                    await ms2.FlushAsync();
                    ms.ToArray().Should().Equal(ms2.ToArray());
                }
            }
        }
        [Test]
        public async Task CreateWriteWithSeekBlob() {
            IRBlobInfo blob = null;
            using (RBlobStream blobStream = await RBlobStream.CreateAsync(_session))
            using (BinaryWriter writer = new BinaryWriter(blobStream)) {
                // write {1, 2, 3}
                writer.Write((long)1);
                writer.Write((long)2);
                writer.Write((long)3);

                // go back to position 2
                blobStream.Seek(sizeof(long), SeekOrigin.Begin);

                // change data to {1, 4, 3}
                writer.Write((long)4);
                blob = blobStream.GetBlobInfo();
            }

            using (RBlobStream blobStream = await RBlobStream.OpenAsync(blob, _session))
            using (BinaryReader reader = new BinaryReader(blobStream)) {
                long[] expectedData = { 1, 4, 3 };
                
                for(int i = 0; i < expectedData.Length; ++i) {
                    reader.ReadInt64().Should().Be(expectedData[i]);
                }
            }
        }

        [Test]
        public async Task ZeroSizedBlob() {
            byte[] data = new byte[] { };
            var blobId = await _session.CreateBlobAsync();
            blobId.Should().BeGreaterThan(0);

            var res = await _session.BlobReadAllAsync(blobId);
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
