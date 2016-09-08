// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
            private readonly TaskObserverMethodFixture _taskObserver;
            private readonly MethodInfo _testMethod;
            private readonly IRHostConnector _hostConnector;
            private readonly RSession _session;

            public Blobs(TestMethodFixture testMethod, TaskObserverMethodFixture taskObserver) {
                _taskObserver = taskObserver;
                _testMethod = testMethod.MethodInfo;
                _hostConnector = CreateLocalConnector(nameof(RSessionTest) + nameof(Blobs));
                _session = new RSession(0, _hostConnector, () => { });
            }

            public async Task InitializeAsync() {
                await _session.StartHostAsync(new RHostStartupInfo {
                    Name = _testMethod.Name
                }, null, 50000);

                _taskObserver.ObserveTaskFailure(_session.RHost.GetRHostRunTask());
            }

            public async Task DisposeAsync() {
                await _session.StopHostAsync();
                _session.Dispose();
                _hostConnector.Dispose();
            }

            [Test]
            public async Task CreateBlob_DisconnectedFromTheStart() {
                using (var session = new RSession(0, _hostConnector, () => { })) {
                    var data = new byte[] { 1, 2, 3, 4, 5 };
                    Func<Task> f = () => session.CreateBlobAsync(data);
                    await f.ShouldThrowAsync<RHostDisconnectedException>();
                }
            }

            [Test]
            public async Task CreateBlob_DisconnectedDuringCreate() {
                var data = new byte[25 * 1024 * 1024]; // try to send a massive blob

                ManualResetEvent testStarted = new ManualResetEvent(false);
                Func<Task> f = () => {
                    testStarted.Set();
                    return _session.CreateBlobAsync(data);
                };
                var assertion = f.ShouldThrowAsync<RHostDisconnectedException>();
                await Task.Delay(100);
                await _session.StopHostAsync();
                await assertion;
            }

            [Test]
            public async Task CreateBlob_CanceledDuringCreate() {
                var cts = new CancellationTokenSource();
                var data = new byte[1024 * 1024];
                Func<Task> f = async () => {
                    while (true) {
                        await _session.CreateBlobAsync(data, ct: cts.Token);
                    }
                };
                var assertion = f.ShouldThrowAsync<TaskCanceledException>();
                cts.CancelAfter(1);
                await assertion;
            }

            [Test]
            public async Task GetBlob_DisconnectedFromTheStart() {
                using (var session = new RSession(0, _hostConnector, () => { })) {
                    Func<Task> f = () => session.GetBlobAsync(1);
                    await f.ShouldThrowAsync<RHostDisconnectedException>();
                }
            }

            [Test]
            public async Task GetBlob_DisconnectedDuringGet() {
                var data = new byte[10 * 1024 * 1024];
                var blobId = await _session.CreateBlobAsync(data);


                Func<Task> f = () => _session.GetBlobAsync(blobId);

                await Task.Delay(100);
                await _session.StopHostAsync();
                await f.ShouldThrowAsync<RHostDisconnectedException>();
            }

            [Test]
            public async Task GetBlob_CanceledDuringGet() {
                var cts = new CancellationTokenSource();
                var data = new byte[1024 * 1024];
                var blobId = await _session.CreateBlobAsync(data);

                Func<Task> f = async () => {
                    while (true) {
                        await _session.GetBlobAsync(blobId, ct: cts.Token);
                    }
                };
                var assertion = f.ShouldThrowAsync<OperationCanceledException>();
                cts.CancelAfter(1);
                await assertion;
            }

            [Test]
            public async Task DestroyBlob_DisconnectedFromTheStart() {
                using (var session = new RSession(0, _hostConnector, () => { })) {
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
