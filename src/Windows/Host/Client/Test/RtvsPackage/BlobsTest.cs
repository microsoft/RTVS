// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.R.Host.Client.Test.RtvsPackage {
    [ExcludeFromCodeCoverage]
    public class BlobsTest : IAsyncLifetime {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public BlobsTest(IServiceContainer services, TestMethodFixture testMethod) {
            _sessionProvider = new RSessionProvider(services);
            _session = _sessionProvider.GetOrCreate(testMethod.FileSystemSafeName);
        }

        public async Task InitializeAsync() {
            await _sessionProvider.TrySwitchBrokerAsync(nameof(BlobsTest));
            await _session.StartHostAsync(new RHostStartupInfo(), new RHostClientTestApp(), 50000);
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
                var blob1 = await dts.SendBytesAsync(data1, true, null, CancellationToken.None);
                var blob2 = await dts.SendBytesAsync(data2, true, null, CancellationToken.None);
                var blob3 = await dts.SendBytesAsync(data3, true, null, CancellationToken.None);

                blob1.Id.Should().BeGreaterThan(0);
                blob2.Id.Should().BeGreaterThan(0);
                blob3.Id.Should().BeGreaterThan(0);

                blob1.Id.Should().NotBe(blob2.Id);
                blob2.Id.Should().NotBe(blob3.Id);
                blob3.Id.Should().NotBe(blob1.Id);

                var blobIds = new IRBlobInfo[] { blob1, blob2, blob3 };

                for (int i = 0; i < blobIds.Length; ++i) {
                    var blob = await dts.FetchBytesAsync(blobIds[i], false, null, CancellationToken.None);
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
            var createResult = await _session.EvaluateAsync("rtvs:::create_blob(as.raw(1:10))", REvaluationKind.Normal);
            createResult.Result.Should().NotBeNull();

            var blobId = ((JValue) createResult.Result).Value<ulong>();
            var actualData = await _session.EvaluateAsync<byte[]>($"rtvs:::get_blob({blobId})", REvaluationKind.Normal);

            byte[] expectedData = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            actualData.Should().Equal(expectedData);

            await _session.ExecuteAsync($"rtvs:::destroy_blob({blobId})");
        }

        [Test]
        public async Task RZeroSizedBlob() {
            var createResult = await _session.EvaluateAsync("rtvs:::create_blob(raw())", REvaluationKind.Normal);
            createResult.Result.Should().NotBeNull();

            var blobId = ((JValue)createResult.Result).Value<ulong>();
            var actualData = await _session.EvaluateAsync<byte[]>($"rtvs:::get_blob({blobId})", REvaluationKind.Normal);

            byte[] expectedData = { };
            actualData.Should().Equal(expectedData);

            await _session.ExecuteAsync($"rtvs:::destroy_blob({blobId})");
        }

        [Test]
        public async Task CompressedBlob() {
            var createResult = await _session.EvaluateAsync("rtvs:::create_blob(raw(1000000))", REvaluationKind.Normal);
            createResult.Result.Should().NotBeNull();

            var createCompressedResult = await _session.EvaluateAsync("rtvs:::create_compressed_blob(raw(1000000))", REvaluationKind.Normal);
            createCompressedResult.Result.Should().NotBeNull();

            var blobId = ((JValue)createResult.Result).Value<ulong>();
            var blobId2 = ((JValue)createCompressedResult.Result).Value<ulong>();

            using (DataTransferSession dts = new DataTransferSession(_session, new WindowsFileSystem())) {
                var expectedData = await dts.FetchBytesAsync(new RBlobInfo(blobId), true, null, CancellationToken.None);
                var compressedData = await dts.FetchBytesAsync(new RBlobInfo(blobId2), true, null, CancellationToken.None);
                compressedData.Length.Should().BeLessThan(expectedData.Length);

                var actualData = await dts.FetchAndDecompressBytesAsync(new RBlobInfo(blobId2), true, null, CancellationToken.None);
                actualData.Should().Equal(expectedData);
            }
        }

        [Test]
        public async Task CompressedZeroSizedBlob() {
            var createResult = await _session.EvaluateAsync("rtvs:::create_blob(raw())", REvaluationKind.Normal);
            createResult.Result.Should().NotBeNull();

            var createCompressedResult = await _session.EvaluateAsync("rtvs:::create_compressed_blob(raw())", REvaluationKind.Normal);
            createCompressedResult.Result.Should().NotBeNull();

            var blobId = ((JValue)createResult.Result).Value<ulong>();
            var blobId2 = ((JValue)createCompressedResult.Result).Value<ulong>();

            using (DataTransferSession dts = new DataTransferSession(_session, new WindowsFileSystem())) {
                var expectedData = await dts.FetchBytesAsync(new RBlobInfo(blobId), true, null, CancellationToken.None);
                var actualData = await dts.FetchAndDecompressBytesAsync(new RBlobInfo(blobId2), true, null, CancellationToken.None);
                actualData.Should().Equal(expectedData);
            }
        }
    }
}
