// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Host.Client.API;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.R.Host.Client.Test.Session {
    [ExcludeFromCodeCoverage]
    [Category.R.Session.Api]
    public sealed class RSessionApiTest : IAsyncLifetime {
        private readonly IRHostSession _session;

        public RSessionApiTest() {
            _session = RHostSession.Create(nameof(RSessionApiTest));
        }

        public async Task InitializeAsync() {
            var cb = Substitute.For<IRHostSessionCallback>();
            await _session.StartHostAsync(cb);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _session?.Dispose();
        }

        [Test]
        public async Task Lifecycle() {
            _session.IsHostRunning.Should().BeTrue();
            _session.IsRemote.Should().BeFalse();

            var result = await _session.EvaluateAsync<int>("1+1");
            result.Should().Be(2);
        }

        [Test]
        public async Task List() {
            await _session.ExecuteAsync("x <- c(1:10)");
            var r1 = await _session.EvaluateAndDescribeAsync("x", TypeNameProperty | ClassesProperty | DimProperty | LengthProperty, CancellationToken.None);
            var r2 = await _session.DescribeChildrenAsync("x", HasChildrenProperty | AccessorKindProperty, null);

            var list = new List<object>();
            foreach(var result in r2) {
                var value = await _session.EvaluateAsync(result.Expression, REvaluationKind.Normal);
                list.Add(value);
            }
        }
    }
}
