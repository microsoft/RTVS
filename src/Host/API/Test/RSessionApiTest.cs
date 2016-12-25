// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Linq;
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
using System.Collections.Generic;

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
            var ol = await _session.GetListAsync("x");
            ol.Count.Should().Be(10);

            var li = ol.ToListOf<int>();
            li.Count.Should().Be(10);
        }

        [Test]
        public async Task Invoke() {
            var func = "f <- function() { c(1:10); }";
            await _session.ExecuteAsync(func);
            var result = await _session.InvokeAndReturnAsync("f", Enumerable.Empty<RFunctionArg>());
            var ol = await _session.GetListAsync(result);

            var li = ol.ToListOf<int>();
            li.Count.Should().Be(10);
        }

        [Test]
        public async Task DataFrame() {
            await _session.ExecuteAsync("x <- datasets::mtcars");
            var df = _session.GetDataFrameAsync("x");
        }
    }
}
