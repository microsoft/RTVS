// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test {
    [ExcludeFromCodeCoverage]
    public class EnumerableExtensionsTest {
        [Test]
        public void AsList_Enumerable() {
            var actual = Enumerable.Range(0, 3).AsList();
            var expected = Enumerable.Range(0, 3).ToList();

            actual.Should()
                .Equal(expected)
                .And
                .BeOfType<List<int>>();
        }

        [Test]
        public void AsList_Array() {
            var actual = Enumerable.Range(0, 3).ToArray().AsList();
            var expected = Enumerable.Range(0, 3).ToList();

            actual.Should()
                .Equal(expected)
                .And
                .BeOfType<List<int>>();
        }

        [Test]
        public void AsList_List() {
            var source = Enumerable.Range(0, 3).ToList();
            var actual = source.AsList();
            var expected = source;

            actual.Should().BeSameAs(expected);
        }

        [Test]
        public void AsArray_Enumerable() {
            var actual = Enumerable.Range(0, 3).AsArray();
            var expected = new[] { 0, 1, 2 };

            actual.Should()
                .Equal(expected)
                .And
                .BeOfType<int[]>();
        }

        [Test]
        public void AsArray_List() {
            var actual = Enumerable.Range(0, 3).ToList().AsArray();
            var expected = new[] { 0, 1, 2 };

            actual.Should()
                .Equal(expected)
                .And
                .BeOfType<int[]>();
        }

        [Test]
        public void AsArray_Array() {
            var expected = new[] { 0, 1, 2 };
            var actual = expected.AsArray();

            actual.Should().BeSameAs(expected);
        }

        [Test]
        public void Append() {
            new[] { 1, 2, 3 }.Concat(new[] { 5 }).Should().Equal(1, 2, 3, 5);
        }

        [Test]
        public void Split() {
            var actual = Enumerable.Range(0, 10).Split(3);
            var expected = new IReadOnlyCollection<int>[]
            {
                new [] { 0, 1, 2 },
                new [] { 3, 4, 5 },
                new [] { 6, 7, 8 },
                new [] { 9 }
            };

            actual.Should().Equal(expected, (a, e) => a.SequenceEqual(e));
        }

        [Test]
        public void Split_Empty() {
            var actual = Enumerable.Empty<int>().Split(3);
            actual.Should().Equal(Enumerable.Empty<IReadOnlyCollection<int>>());
        }

        [Test]
        public void IndexWhere() {
            var actual = Enumerable.Range(2, 10).IndexWhere(v => v % 3 == 0);
            actual.Should().Equal(1, 4, 7);
        }

        [Test]
        public void TraverseBreadthFirst() {
            var tree = new TreeItem(0, new[]
            {
                new TreeItem(1, new[] { new TreeItem(2), new TreeItem(3), new TreeItem(4) }),
                new TreeItem(5, new[] {
                    new TreeItem(6),
                    new TreeItem(7, new []{ new TreeItem(8), new TreeItem(9) })
                }),
                new TreeItem(10, new[] { new TreeItem(11) }),
            });

            var actual = tree.TraverseBreadthFirst(ti => ti.Children);
            actual.Select(i => i.Value).Should().Equal(0, 1, 5, 10, 2, 3, 4, 6, 7, 11, 8, 9);
        }

        [Test]
        public void TraverseDepthFirst() {
            var tree = new TreeItem(0, new[]
            {
                new TreeItem(1, new[] { new TreeItem(2), new TreeItem(3), new TreeItem(4) }),
                new TreeItem(5, new[] {
                    new TreeItem(6),
                    new TreeItem(7, new []{ new TreeItem(8), new TreeItem(9) })
                }),
                new TreeItem(10, new[] { new TreeItem(11) }),
            });

            var actual = tree.TraverseDepthFirst(ti => ti.Children);
            actual.Select(i => i.Value).Should().Equal(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
        }

        [ExcludeFromCodeCoverage]
        public class TreeItem {
            public int Value { get; }
            public IEnumerable<TreeItem> Children { get; }

            public TreeItem(int value) {
                Value = value;
                Children = Enumerable.Empty<TreeItem>();
            }

            public TreeItem(int value, IEnumerable<TreeItem> children) {
                Value = value;
                Children = children;
            }
        }
    }
}
