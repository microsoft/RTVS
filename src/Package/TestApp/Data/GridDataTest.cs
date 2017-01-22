// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using Xunit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Data {
    // TODO: determine and remove any duplicates from EvaluationWrapperTest.
    // TODO: move this to Microsoft.R.Host.Client.Test after removing GridData dependencies on Package.
    [ExcludeFromCodeCoverage]
    public class GridDataTest : IAsyncLifetime {
        private struct GridElement<T> {
            public int X, Y;
            public T Value;

            public override string ToString() => $"[{Y}, {X}] = {Value}";
        }

        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public GridDataTest(CoreServicesFixture coreServices, TestMethodFixture testMethod) {
            _sessionProvider = new RSessionProvider(coreServices);
            _session = _sessionProvider.GetOrCreate(testMethod.FileSystemSafeName);
        }

        public async Task InitializeAsync() {
            await _sessionProvider.TrySwitchBrokerAsync(GetType().Name);
            await _session.StartHostAsync(new RHostStartupInfo(), new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        private static IEnumerable<T> ToEnumerable<T>(IRange<T> range) {
            int start = range.Range.Start, end = start + range.Range.Count;
            for (int i = start; i < end; ++i) {
                yield return range[i];
            }
        }

        private void ShouldEqual<T>(IGrid<T> actual, T[,] expected) {
            var expectedElements = new List<GridElement<T>>();
            for (int y = 0; y <= expected.GetUpperBound(0); ++y) {
                for (int x = 0; x <= expected.GetUpperBound(1); ++x) {
                    expectedElements.Add(new GridElement<T> {
                        X = x + actual.Range.Columns.Start,
                        Y = y + actual.Range.Rows.Start,
                        Value = expected[y, x]
                    });
                }
            }

            var actualElements = new List<GridElement<T>>();
            foreach (int y in actual.Range.Rows.GetEnumerable()) {
                foreach (int x in actual.Range.Columns.GetEnumerable()) {
                    actualElements.Add(new GridElement<T> {
                        X = x,
                        Y = y,
                        Value = actual[y, x]
                    });
                }
            }

            actualElements.Should().Equal(expectedElements);
        }

        /// <summary>
        /// Runs <c>rtvs:::grid_data</c> for <paramref name="expression"/>, and validates that
        /// returned data matches expectations.
        /// </summary>
        /// <param name="expression">Expression to produce a data structure to retrieve grid data from.</param>
        /// <param name="firstRow">First row which should be retrieved; 1-based (as in R).</param>
        /// <param name="firstColumn">First column that should be retrieved; 1-based (as in R).</param>
        /// <param name="expected">
        /// Expected values. This is a 2D array of strings.
        /// Element at <c>[0, 0]</c> must be <see langword="null"/>.
        /// Elements at <c>[0, 1]</c>, <c>[0, 2]</c> etc specify expected column names.
        /// Elements at <c>[1, 0]</c>, <c>[2, 0]</c> etc specify expected row names.
        /// All other elements specify expected values in the corresponding grid cells.
        /// </param>
        /// <param name="sort">
        /// Indices of columns that the data should be sorted on; 1-based (as in R).
        /// A positive value indicates that the corresponding column should be sorted in ascending order.
        /// A negative value indicates that the corresponding column should be sorted in descending order.
        /// If <see langword="null"/>, no sorting is done.
        /// </param>
        /// <remarks>
        /// <para>
        /// The number of rows and columns that are fetched is the same as the number of elements
        /// in <paramref name="expected"/>, not counting the row and column headers. For example,
        /// if <paramref name="expected"/> is a 3x4 array, a 2x3 slice is fetched, with the top left
        /// corner specified by <paramref name="firstRow"/> and <paramref name="firstColumnt"/>.
        /// </para>
        /// <para>
        /// <c>rtvs:::grid_data</c> sorts the entire dataset first, and only then slices it.
        /// </para>
        /// <remarks>
        private async Task Test(
            string expression,
            int firstRow,
            int firstColumn,
            string[,] expected,
            int[] sort = null
        ) {
            int height = expected.GetUpperBound(0) + 1;
            int width = expected.GetUpperBound(1) + 1;

            height.Should().BeGreaterOrEqualTo(1);
            width.Should().BeGreaterOrEqualTo(1);
            expected[0, 0].Should().BeNull();

            var expectedRowHeaders = new List<string>();
            for (int y = 1; y < height; ++y) {
                expectedRowHeaders.Add(expected[y, 0]);
            }

            var expectedColumnHeaders = new List<string>();
            for (int x = 1; x < width; ++x) {
                expectedColumnHeaders.Add(expected[0, x]);
            }

            var expectedValues = new string[height - 1, width - 1];
            for (int y = 1; y < height; ++y) {
                for (int x = 1; x < width; ++x) {
                    expectedValues[y - 1, x - 1] = expected[y, x];
                }
            }

            var range = new GridRange(
                new Range(firstRow - 1, expectedRowHeaders.Count),
                new Range(firstColumn - 1, expectedColumnHeaders.Count));

            SortOrder order = null;
            if (sort != null) {
                order = new SortOrder();
                foreach (var x in sort) {
                    order.Add(new ColumnSortOrder(Math.Abs(x) - 1, x < 0));
                }
            }
         
            var data = await GridDataSource.GetGridDataAsync(_session, expression, range, order);

            ToEnumerable(data.RowHeader).Should().Equal(expectedRowHeaders);
            ToEnumerable(data.ColumnHeader).Should().Equal(expectedColumnHeaders);

            data.Grid.Range.Rows.Count.Should().Be(expectedRowHeaders.Count);
            data.Grid.Range.Columns.Count.Should().Be(expectedColumnHeaders.Count);

            ShouldEqual(data.Grid, expectedValues);
        }

        [Test]
        [Category.R.DataGrid]
        public Task DataFrameGrid() => Test("iris", 48, 3, new[,] {
            { null, "Petal.Length", "Petal.Width",  "Species" },
            { "48", "1.4",          "0.2",          "setosa" },
            { "49", "1.5",          "0.2",          "setosa" },
            { "50", "1.4",          "0.2",          "setosa" },
            { "51", "4.7",          "1.4",          "versicolor" },
            { "52", "4.5",          "1.5",          "versicolor" },
        });

        [Test]
        [Category.R.DataGrid]
        public Task DataFrameNAGrid() => Test("df.test <- data.frame(c(1, as.integer(NA)), c(2.0, as.double(NA)), c(as.Date('2011-12-31'), as.Date(NA)))", 1, 1, new[,] {
            { null, "c.1..as.integer.NA..", "c.2..as.double.NA..",  "c.as.Date..2011.12.31....as.Date.NA.." },
            { "1",  "1",                    "2",                    "2011-12-31" },
            { "2",  "NA",                   "NA",                   "NA" },
        });

        [Test]
        [Category.R.DataGrid]
        public Task DataFrameSortedGrid() => Test("iris", 48, 3, new[,] {
            { null, "Petal.Length", "Petal.Width",  "Species" },
            { "24", "1.7",          "0.5",          "setosa" },
            { "45", "1.9",          "0.4",          "setosa" },
            { "25", "1.9",          "0.2",          "setosa" },
            { "99", "3.0",          "1.1",          "versicolor" },
            { "58", "3.3",          "1.0",          "versicolor" },
        }, sort: new[] { +3, -2 });

        [Test]
        [Category.R.DataGrid]
        public Task MatrixGrid() => Test("matrix(1:20, 5, 4)", 2, 2, new[,] {
            { null,     "[,2]",     "[,3]" },
            { "[2,]",   "7",        "12" },
            { "[3,]",   "8",        "13" },
            { "[4,]",   "9",        "14" },
        });

        [Test]
        [Category.R.DataGrid]
        public Task MatrixSortedGrid() => Test("matrix(1:20, 5, 4)", 2, 2, new[,] {
            { null,     "[,2]",     "[,3]" },
            { "[2,]",   "9",        "14" },
            { "[3,]",   "8",        "13" },
            { "[4,]",   "7",        "12" },
        }, sort: new[] { -2 });

        [Test]
        [Category.R.DataGrid]
        public Task MatrixCharSortedGrid() => Test("matrix(c('a', 'b', 'c', 1, 1, 2), 3, 2)", 1, 1, new[,] {
            { null,     "[,1]",     "[,2]" },
            { "[1,]",   "c",        "2" },
            { "[2,]",   "b",        "1" },
            { "[3,]",   "a",        "1" },
        }, sort: new[] { -2, -1 });

        [Test]
        [Category.R.DataGrid]
        public Task VectorGrid() => Test("1:10", 2, 1, new[,] {
            { null,     "[]" },
            { "[2]",    "2"  },
            { "[3]",    "3"  },
            { "[4]",    "4"  },
        });

        [Test]
        [Category.R.DataGrid]
        public Task VectorSortedGrid() => Test("1:10", 2, 1, new[,] {
            { null,     "[]" },
            { "[2]",    "9"  },
            { "[3]",    "8"  },
            { "[4]",    "7"  },
        }, sort: new[] { -1 });

        [Test]
        [Category.R.DataGrid]
        public Task ListGrid() => Test("as.list(1:10)", 2, 1, new[,] {
            { null,     "[]" },
            { "[2]",    "2"  },
            { "[3]",    "3"  },
            { "[4]",    "4"  },
        });

        [Test]
        [Category.R.DataGrid]
        public Task ArrayGrid() => Test("array(1:10)", 2, 1, new[,] {
            { null,     "[]" },
            { "[2]",    "2"  },
            { "[3]",    "3"  },
            { "[4]",    "4"  },
        });

        [Test]
        [Category.R.DataGrid]
        public Task ArraySortedGrid() => Test("array(1:10)", 2, 1, new[,] {
            { null,     "[]" },
            { "[2]",    "9"  },
            { "[3]",    "8"  },
            { "[4]",    "7"  },
        }, sort: new[] { -1 });


        [Test]
        [Category.R.DataGrid]
        public Task GridUnsortableColumn() => Test("matrix(list(2, 'a', 1, 20, 10, 20), 3, 2)", 1, 1, new[,] {
            { null,     "[,1]",     "[,2]" },
            { "[1,]",   "2",        "20" },
            { "[2,]",   "a",        "10" },
            { "[3,]",   "1",        "20" },
        }, sort: new[] { 1, 2 });

        [Test]
        [Category.R.DataGrid]
        public Task ExternalPtrGrid() => Test("matrix(list(1, .Internal(address(2)), 3, 4), 2, 2)", 1, 1, new[,] {
            { null,     "[,1]",             "[,2]" },
            { "[1,]",   "1",                "3" },
            { "[2,]",   "<externalptr> ",    "4" },
        });

    }
}
