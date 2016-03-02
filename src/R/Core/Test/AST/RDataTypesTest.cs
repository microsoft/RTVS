// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.AST {
    [ExcludeFromCodeCoverage]
    public class RDataTypesTest {
        [Test]
        [Category.R.Ast]
        public void RObjectTest() {
            RNumber rn = new RNumber(1);
            RLogical rl = new RLogical(false);
            RString rs = new RString("abc");

            RVector<RString> rvs = new RVector<RString>(RMode.Character, 1);
            RVector<RNumber> rvn = new RVector<RNumber>(RMode.Numeric, 2);

            rs.IsString.Should().BeTrue();
            rn.IsString.Should().BeFalse();
            rl.IsString.Should().BeFalse();

            rs.IsNumber.Should().BeFalse();
            rn.IsNumber.Should().BeTrue();
            rl.IsNumber.Should().BeFalse();

            rs.IsScalar.Should().BeTrue();
            rn.IsScalar.Should().BeTrue();
            rl.IsScalar.Should().BeTrue();

            rs.IsBoolean.Should().BeFalse();
            rn.IsBoolean.Should().BeFalse();
            rl.IsBoolean.Should().BeTrue();

            rvs.IsScalar.Should().BeTrue();
            rvn.IsScalar.Should().BeFalse();

            rvs.IsString.Should().BeTrue();
            rvn.IsNumber.Should().BeFalse();
        }

        [Test]
        [Category.R.Ast]
        public void RLogicalTest() {
            RLogical rlTrue1 = new RLogical(true);
            RLogical rlTrue2 = new RLogical(true);
            RLogical rlFalse1 = new RLogical(false);
            RLogical rlFalse2 = new RLogical(false);

            rlTrue1.Mode.Should().Be(RMode.Logical);
            (rlTrue1 == true).Should().BeTrue();
            (rlTrue1 != true).Should().BeFalse();
            (rlTrue1 == false).Should().BeFalse();
            ((bool)rlTrue1).Should().BeTrue();
            rlTrue2.Should().Be(rlTrue1);

            rlFalse1.Mode.Should().Be(RMode.Logical);
            (rlFalse1 == false).Should().BeTrue();
            (rlFalse1 != false).Should().BeFalse();
            (rlFalse1 == true).Should().BeFalse();
            ((bool)rlFalse1).Should().BeFalse();
            rlFalse2.Should().Be(rlFalse1);
            rlTrue1.Should().NotBe(rlFalse1);

            (rlFalse1 == rlFalse2).Should().BeTrue();
            (rlFalse1 != rlTrue1).Should().BeTrue();
            (rlTrue1 == rlTrue2).Should().BeTrue();

            ((bool)(rlFalse1 & rlFalse2)).Should().BeFalse();
            ((bool)(rlFalse1 & rlTrue1)).Should().BeFalse();
            ((bool)(rlFalse1 | rlFalse2)).Should().BeFalse();
            ((bool)(rlFalse1 | rlTrue1)).Should().BeTrue();

            ((bool)!rlFalse1).Should().BeTrue();
            ((bool)!rlTrue1).Should().BeFalse();
        }

        [Test]
        [Category.R.Ast]
        public void RNumberTest() {
            RNumber rn1 = new RNumber(1);
            RNumber rn2 = new RNumber(1.0);
            RNumber rn3 = new RNumber(2.1);

            rn1.Mode.Should().Be(RMode.Numeric);
            (rn1 == 1).Should().BeTrue();
            (rn1 != 1).Should().BeFalse();
            (rn1 == 2).Should().BeFalse();
            ((double)rn1).Should().Be(1);
            rn2.Should().Be(rn1);
            rn3.Should().NotBe(rn2);
            (rn1 != rn3).Should().BeTrue();

            (rn1 * rn2).Should().Be((RNumber)1);
            (rn2 + rn3).Should().Be((RNumber)3.1);
            (rn3 - rn1).Should().Be((RNumber)1.1);
            (rn3 / 2).Should().Be((RNumber)1.05);
        }

        [Test]
        [Category.R.Ast]
        public void RIntegerTest() {
            RInteger rn1 = new RInteger(1);
            RInteger rn2 = new RInteger(1);
            RInteger rn3 = new RInteger(2);

            rn1.Mode.Should().Be(RMode.Numeric);
            (rn1 == 1).Should().BeTrue();
            (rn1 != 1).Should().BeFalse();
            (rn1 == 2).Should().BeFalse();
            ((double)rn1).Should().Be(1);
            rn2.Should().Be(rn1);
            rn3.Should().NotBe(rn2);
            (rn1 != rn3).Should().BeTrue();
        }

        [Test]
        [Category.R.Ast]
        public void RVectorTest() {
            var rv = new RVector<RLogical>(RMode.Logical, 2);
            rv.Length.Should().Be(2);
            rv.Mode.Should().Be(RMode.Logical);
            rv.IsBoolean.Should().BeFalse();

            rv[0] = new RLogical(false);
            rv[1] = new RLogical(true);

            rv[0].Should().Be(RLogical.FALSE);
            rv[1].Should().Be(RLogical.TRUE);

            rv = new RVector<RLogical>(RMode.Logical, 1);
            rv.IsBoolean.Should().BeTrue();
        }

        [Test]
        [Category.R.Ast]
        public void RListTest() {
            RList rl = new RList();

            rl.Mode.Should().Be(RMode.List);
            rl.Should().BeEmpty();

            var e = rl.GetEnumerator();
            e.Should().NotBeNull();

            e.MoveNext().Should().BeFalse();

            RObject rv = new RVector<RNumber>(RMode.Numeric, 1);
            var rs = new RString("abc");

            rl.Add(rs, rv);
            rl.Should().HaveCount(1);

            var e1 = rl.Keys.GetEnumerator();
            e1.MoveNext();
            e1.Current.Should().Be(rs);
            e1.Current.Should().Be(new RString("abc"));

            var e2 = rl.Values.GetEnumerator();
            e2.MoveNext();
            e2.Current.Should().Be(rv);

            rl.ContainsKey(rs).Should().BeTrue();
            rl.ContainsKey(new RString("abc")).Should().BeTrue();

            rl.Contains(new KeyValuePair<RString, RObject>(rs, rv)).Should().BeTrue();

            var arr = new KeyValuePair<RString, RObject>[2];
            rl.CopyTo(arr, 1);

            arr[1].Key.Should().Be(rs);
            arr[1].Value.Should().Be(rv);

            rl[rs].Should().Be(rv);
            rl.IsReadOnly.Should().BeFalse();

            RObject u;
            rl.TryGetValue(rs, out u).Should().BeTrue();

            var en = rl.GetEnumerator();
            en.Should().NotBeNull();
            en.MoveNext().Should().BeTrue();
            en.Current.Key.Should().Be(rs);
            en.Current.Value.Should().Be(rv);
            en.MoveNext().Should().BeFalse();

            IEnumerator<RObject> en1 = ((IEnumerable<RObject>)rl).GetEnumerator();
            en1.Should().NotBeNull();
            en1.MoveNext().Should().BeTrue();
            en1.Current.Should().Be(rv);
            en1.MoveNext().Should().BeFalse();

            IEnumerator<KeyValuePair<RString, RObject>> en2 = ((IEnumerable<KeyValuePair<RString, RObject>>)rl).GetEnumerator();
            en2.Should().NotBeNull();
            en2.MoveNext().Should().BeTrue();
            en2.Current.Key.Should().Be(rs);
            en2.Current.Value.Should().Be(rv);
            en2.MoveNext().Should().BeFalse();

            IEnumerator en3 = ((IEnumerable)rl).GetEnumerator();
            en3.Should().NotBeNull();
            en3.MoveNext().Should().BeTrue();
            en3.MoveNext().Should().BeFalse();

            rl.Remove(rs).Should().BeTrue();
            rl.Should().BeEmpty();
            rl.ContainsKey(rs).Should().BeFalse();

            rl.Add(new KeyValuePair<RString, RObject>(new RString("x"), new RLogical(true)));
            rl.Length.Should().Be(1);
            rl.Count.Should().Be(1);
            rl.Clear();
            rl.Length.Should().Be(0);
            rl.Count.Should().Be(0);

            rl.TryGetValue(rs, out u).Should().BeFalse();
            u.Should().BeNull();
        }
    }
}
