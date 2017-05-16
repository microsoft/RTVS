// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Testing;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.Host.Client.Test.Session {
    public partial class RSessionTest {
        [Category.R.Session]
        public class Blobs : IAsyncLifetime {
            private readonly TestMethodFixture _testMethod;
            private readonly IBrokerClient _brokerClient;
            private readonly RSession _session;

            public Blobs(IServiceContainer services, TestMethodFixture testMethod) {
                _testMethod = testMethod;
                _brokerClient = CreateLocalBrokerClient(services, nameof(RSessionTest) + nameof(Blobs));
                _session = new RSession(0, testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });
            }

            public async Task InitializeAsync() {
                await _session.StartHostAsync(new RHostStartupInfo(), null, 50000);
                TestEnvironment.Current.TryAddTaskToWait(_session.RHost.GetRHostRunTask());
            }

            public async Task DisposeAsync() {
                await _session.StopHostAsync();
                _session.Dispose();
                _brokerClient.Dispose();
            }

            [Test]
            public async Task CreateBlob_DisconnectedFromTheStart() {
                using (var session = new RSession(0, _testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { })) {
                    var data = new byte[] { 1, 2, 3, 4, 5 };
                    using (DataTransferSession dts = new DataTransferSession(session, null)) {
                        Func<Task> f = () => dts.SendBytesAsync(data, false, null, CancellationToken.None);
                        await f.ShouldThrowAsync<RHostDisconnectedException>();
                    }
                }
            }

            [Test]
            public async Task CreateBlob_DisconnectedDuringCreate() {
                var data = new byte[25 * 1024 * 1024]; // try to send a massive blob

                using (DataTransferSession dts = new DataTransferSession(_session, null)) {
                    Func<Task> f = () => dts.SendBytesAsync(data, false, null, CancellationToken.None);
                    var assertion = f.ShouldThrowAsync<RHostDisconnectedException>();
                    await Task.Delay(100);
                    await _session.StopHostAsync();
                    await assertion;
                }
            }

            [Test]
            public async Task CreateBlob_CanceledDuringCreate() {
                var cts = new CancellationTokenSource();
                var data = new byte[1024 * 1024];
                Func<Task> f = async () => {
                    while (true) {
                        var blob = await _session.CreateBlobAsync(ct: cts.Token);
                        await _session.BlobWriteAsync(blob, data, -1, ct: cts.Token);
                    }
                };
                var assertion = f.ShouldThrowAsync<OperationCanceledException>();
                cts.CancelAfter(1);
                await assertion;
            }

            [Test]
            public async Task GetBlob_DisconnectedFromTheStart() {
                using (var session = new RSession(0, _testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { })) {
                    Func<Task> f = () => session.BlobReadAllAsync(1);
                    await f.ShouldThrowAsync<RHostDisconnectedException>();
                }
            }

            [Test]
            public async Task GetBlob_DisconnectedDuringGet() {
                var data = new byte[1024 * 1024];

                IRBlobInfo blob = null;
                using (DataTransferSession dts = new DataTransferSession(_session, null)) {
                    blob = await dts.SendBytesAsync(data, false, null, CancellationToken.None);
                }

                using (RBlobStream blobStream = await RBlobStream.OpenAsync(blob, _session)) {
                    Func<Task> f = async () => {
                        using (MemoryStream ms = new MemoryStream()) {
                            await blobStream.CopyToAsync(ms, 1024);
                        }
                    };
                    await Task.Delay(100);
                    await _session.StopHostAsync();
                    await f.ShouldThrowAsync<RHostDisconnectedException>();
                }
            }

            [Test]
            public async Task GetBlob_CanceledDuringGet() {
                var cts = new CancellationTokenSource();
                var data = new byte[1024 * 1024];

                IRBlobInfo blob = null;
                using (DataTransferSession dts = new DataTransferSession(_session, null)) {
                    blob = await dts.SendBytesAsync(data, false, null, CancellationToken.None);
                }

                Func<Task> f = async () => {
                    while (true) {
                        await _session.BlobReadAllAsync(blob.Id, ct: cts.Token);
                    }
                };
                var assertion = f.ShouldThrowAsync<OperationCanceledException>();
                cts.CancelAfter(1);
                await assertion;
            }

            [Test]
            public async Task DestroyBlob_DisconnectedFromTheStart() {
                using (var session = new RSession(0, _testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { })) {
                    var blobids = new ulong[] { 1, 2, 3, 4, 5 };
                    Func<Task> f = () => session.DestroyBlobsAsync(blobids);
                    await f.ShouldThrowAsync<RHostDisconnectedException>();
                }
            }

            [Test]
            public async Task DestroyBlob_DisconnectedDuringDestroy() {
                var blobIds = new ulong[1024 * 1024];

                Func<Task> f = () => _session.DestroyBlobsAsync(blobIds);

                var assertion = Task.Run(() => f.ShouldThrowAsync<RHostDisconnectedException>());
                await _session.StopHostAsync();
                await assertion;
            }

            [Test]
            public async Task DestroyBlob_CanceledDuringDestroy() {
                var cts = new CancellationTokenSource();
                Func<Task> f = async () => {
                    var blobIds = new ulong[1024 * 1024];
                    await _session.DestroyBlobsAsync(blobIds, ct: cts.Token);
                };
                var assertion = f.ShouldThrowAsync<TaskCanceledException>();
                cts.CancelAfter(1);
                await assertion;
            }

        }
    }
}
