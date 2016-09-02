// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using FluentAssertions;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Host.Client.Test.Session {
    public class RSessionProviderTest {
        [Test]
        public void Lifecycle() {
            var sessionProvider = new RSessionProvider();
            // ReSharper disable once AccessToDisposedClosure
            Action a = () => sessionProvider.GetOrCreate(new Guid());
            a.ShouldNotThrow();

            sessionProvider.Dispose();
            a.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void GetOrCreate() {
            using (var sessionProvider = new RSessionProvider()) {
                var guid = new Guid();
                var session1 = sessionProvider.GetOrCreate(guid);
                session1.Should().NotBeNull();

                var session2 = sessionProvider.GetOrCreate(guid);
                session2.Should().BeSameAs(session1);

                session1.Dispose();
                var session3 = sessionProvider.GetOrCreate(guid);
                session3.Should().NotBeSameAs(session1);
                session3.Id.Should().NotBe(session1.Id);
            }
        }

        [Test]
        public void ParallelAccess() {
            using (var sessionProvider = new RSessionProvider()) {
                var guids = new[] { new Guid(), new Guid() };
                ParallelTools.Invoke(100, i => {
                    var session = sessionProvider.GetOrCreate(guids[i % 2]);
                    session.Dispose();
                });
            }
        }
    }
}
