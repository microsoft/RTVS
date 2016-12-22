// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.API;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using NSubstitute;

namespace Microsoft.R.Host.Client.Test.Session {
    [ExcludeFromCodeCoverage]
    public sealed class RSessionApiTest : IDisposable {
        private readonly MethodInfo _testMethod;
        private IRHostSession _session;

        public RSessionApiTest(TestMethodFixture testMethod) {
            _testMethod = testMethod.MethodInfo;
            _session = RHostSession.Create(nameof(RSessionApiTest));
        }

        public void Dispose() {
            _session?.Dispose();
        }

        [Test]
        [Category.R.Session.Api]
        public async Task Lifecycle() {
            _session.IsHostRunning.Should().BeFalse();

            var cb = Substitute.For<IRHostSessionCallback>();
            await _session.StartHostAsync(cb);

            _session.IsHostRunning.Should().BeTrue();
            _session.IsRemote.Should().BeFalse();

            var result = await _session.EvaluateAsync<int>("1+1", REvaluationKind.Normal);
            result.Should().Be(2);
        }
    }
}
