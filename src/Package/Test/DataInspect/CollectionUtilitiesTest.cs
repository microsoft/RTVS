// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [Category.Variable.Explorer]
    public class CollectionUtilitiesTest {
        [Test]
        public void InplaceUpdateAddTest() {
            var source = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(3) };
            var update = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);
            source.Should().Equal(update, IntegerComparer);

            source[0].Updated.Should().BeTrue();
            source[1].Updated.Should().BeFalse();
            source[2].Updated.Should().BeTrue();
            source[3].Updated.Should().BeFalse();
        }

        [Test]
        public void InplaceUpdateRemoveTest() {
            var source = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };
            var update = new List<IntegerWrap>() { new IntegerWrap(2), new IntegerWrap(4) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);
            source.Should().Equal(update, IntegerComparer);

            source[0].Updated.Should().BeTrue();
            source[1].Updated.Should().BeTrue();
        }

        [Test]
        public void InplaceUpdateMixedTest() {
            var source = new List<IntegerWrap>() { new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };
            var update = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);
            source.Should().Equal(update, IntegerComparer);

            source[0].Updated.Should().BeFalse();
            source[1].Updated.Should().BeTrue();
        }

        [Test]
        public void InplaceUpdateRemoveAllTest() {
            var source = new List<IntegerWrap> { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3) };
            var update = new List<IntegerWrap>();

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);
            source.Should().Equal(update, IntegerComparer);
        }

        [Test]
        public void InplaceUpdateAddToEmptyTest() {
            var source = new List<IntegerWrap>();
            List<IntegerWrap> update = new List<IntegerWrap> { new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);
            source.Should().Equal(update, IntegerComparer);

            source[0].Updated.Should().BeFalse();
            source[1].Updated.Should().BeFalse();
            source[2].Updated.Should().BeFalse();
        }

        private bool IntegerComparer(IntegerWrap value1, IntegerWrap value2) {
            return value1.Value == value2.Value;
        }

        private void ElementUpdater(IntegerWrap source, IntegerWrap target) {
            source.Value = target.Value;
            source.Updated = true;
        }

        class IntegerWrap {
            public IntegerWrap(int value) {
                Value = value;
                Updated = false;
            }

            public int Value { get; set; }
            public bool Updated { get; set; }

            public override string ToString() {
                return $"{Value} {Updated}";
            }
        }
    }
}
