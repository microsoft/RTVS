// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Host.Client.Test.Session {
    [ExcludeFromCodeCoverage]
    [Category.R.Session.Api]
    public sealed class RSessionApiTest : IAsyncLifetime {
        private readonly IRHostSession _session;
        private readonly IRHostSessionCallback _callback;

        public RSessionApiTest() {
            _callback = Substitute.For<IRHostSessionCallback>();
            _session = RHostSession.Create(nameof(RSessionApiTest));
        }

        public async Task InitializeAsync() {
            await _session.StartHostAsync(_callback);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _session.Dispose();
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
            var ol = await _session.GetListAsync<int>("x");

            ol.Count.Should().Be(10);
            ol.Should().ContainInOrder(new int[] { 1, 2, 3, 4, 5, 6, 7, 9, 10 });
        }

        [Test]
        public async Task InvokeReturnList() {
            var func = "f <- function() { c(1:10); }";
            await _session.ExecuteAsync(func);

            var result = await _session.InvokeAndReturnAsync("f");
            var ol = await _session.GetListAsync<int>(result);

            ol.Count.Should().Be(10);
            ol.Should().ContainInOrder(new int[] { 1, 2, 3, 4, 5, 6, 7, 9, 10 });
        }

        [Test]
        public async Task InvokeSimpleParams() {
            var func = "f <- function(a, b) { a + b }";
            await _session.ExecuteAsync(func);
            var result = await _session.InvokeAndReturnAsync("f", CancellationToken.None, 1, 2);

            var r = await _session.EvaluateAsync<int>(result);
            r.Should().Be(3);
        }

        [CompositeTest]
        [InlineData(new object[] { "a", "b" }, new object[] { 1, 2 }, new object[] { "a", "b", "1", "2" })]
        [InlineData(new object[] { 1, 2 }, new object[] { 3, 4 }, new object[] { 1, 2, 3, 4 })]
        [InlineData(new object[] { null, 2.1 }, new object[0], new object[] { 2.1 })]
        public async Task InvokeListParams(object[] list1, object[] list2, object[] expected) {
            var func = "f <- function(a, b) { c(a, b) }";
            await _session.ExecuteAsync(func);
            var result = await _session.InvokeAndReturnAsync("f", CancellationToken.None, list1, list2);

            var list = await _session.GetListAsync(result);
            list.Should().HaveCount(expected.Length).And.ContainInOrder(expected);
        }

        [Test]
        public async Task GetDataFrame() {
            await _session.ExecuteAsync("x <- mtcars");

            var df = await _session.GetDataFrameAsync("x");
            df.RowNames.First().Should().Be("Mazda RX4");
            df.ColumnNames.Should().HaveCount(11);

            var colData = df.GetColumn("mpg");
            colData.Should().HaveCount(32);
            colData[0].Should().Be(21);
        }

        [CompositeTest]
        [InlineData(new object[] { 1, "a" }, new object[] { "1", "a" })]
        [InlineData(new object[] { null, "a" }, new object[] { "a" })]
        public async Task CreateList(object[] data, object[] expected) {
            await _session.CreateListAsync("x", data);

            var list = await _session.GetListAsync("x");
            list.Should().HaveCount(expected.Length).And.ContainInOrder(expected);
        }

        [CompositeTest]
        [InlineData(new string[] { "row1", "row2" }, new string[] { "col1", "col2" }, new object[] { new object[] { 1, 2 }, new object[] { "a", "b" } })]
        public async Task CreateDataFrame(string[] rowNames, string[] colNames, object[] data) {
            var list = new List<IReadOnlyCollection<object>>();
            foreach (object o in data) {
                list.Add(o as object[]);
            }
            var original = new DataFrame(rowNames, colNames, list.AsReadOnly());
            await _session.CreateDataFrameAsync("x", original);
            var rdf = await _session.GetDataFrameAsync("x");

            rdf.RowNames.Should().HaveCount(rowNames.Length).And.ContainInOrder(rowNames);
            rdf.ColumnNames.Should().HaveCount(colNames.Length).And.ContainInOrder(colNames);
            rdf.Data.Should().HaveCount(colNames.Length);
            rdf.Data.First().Should().HaveCount(rowNames.Length).And.ContainInOrder(data[0] as object[]);
        }


        [Test]
        public async Task PlotAsync() {
            var data = await _session.PlotAsync("c(1:10)", 480, 480, 72);
            data.Length.Should().BeGreaterThan(0);
        }

        [CompositeTest]
        [InlineData("1+1", false, "[1] 2\n")]
        [InlineData("x123", true, "Error: object 'x123' not found\n")]
        public async Task OutputAsync(string expression, bool isError, string expected) {
            var output = await _session.ExecuteAndOutputAsync(expression);
            if (isError) {
                output.Errors.Should().Be(expected);
            } else {
                output.Output.Should().Be(expected);
            }
        }
    }
}
