using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.DataTypes.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.AST
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RDataTypesTest : UnitTestBase
    {
        [TestMethod]
        public void RObjectTest()
        {
            RNumber rn = new RNumber(1);
            RLogical rl = new RLogical(false);
            RString rs = new RString("abc");

            RVector<RString> rvs = new RVector<RString>(RMode.Character, 1);
            RVector<RNumber> rvn = new RVector<RNumber>(RMode.Numeric, 2);

            Assert.IsTrue(rs.IsString);
            Assert.IsFalse(rn.IsString);
            Assert.IsFalse(rl.IsString);

            Assert.IsFalse(rs.IsNumber);
            Assert.IsTrue(rn.IsNumber);
            Assert.IsFalse(rl.IsNumber);

            Assert.IsTrue(rs.IsScalar);
            Assert.IsTrue(rn.IsScalar);
            Assert.IsTrue(rl.IsScalar);

            Assert.IsFalse(rs.IsBoolean);
            Assert.IsFalse(rn.IsBoolean);
            Assert.IsTrue(rl.IsBoolean);

            Assert.IsTrue(rvs.IsScalar);
            Assert.IsFalse(rvn.IsScalar);

            Assert.IsTrue(rvs.IsString);
            Assert.IsFalse(rvn.IsNumber);
        }

        [TestMethod]
        public void RLogicalTest()
        {
            RLogical rlTrue1 = new RLogical(true);
            RLogical rlTrue2 = new RLogical(true);
            RLogical rlFalse1 = new RLogical(false);
            RLogical rlFalse2 = new RLogical(false);

            Assert.AreEqual(RMode.Logical, rlTrue1.Mode);
            Assert.IsTrue(rlTrue1 == true);
            Assert.IsFalse(rlTrue1 != true);
            Assert.IsFalse(rlTrue1 == false);
            Assert.IsTrue((bool)rlTrue1);
            Assert.AreEqual(rlTrue1, rlTrue2);

            Assert.AreEqual(RMode.Logical, rlFalse1.Mode);
            Assert.IsTrue(rlFalse1 == false);
            Assert.IsFalse(rlFalse1 != false);
            Assert.IsFalse(rlFalse1 == true);
            Assert.IsFalse((bool)rlFalse1);
            Assert.AreEqual(rlFalse1, rlFalse2);
            Assert.AreNotEqual(rlFalse1, rlTrue1);

            Assert.IsTrue(rlFalse1 == rlFalse2);
            Assert.IsTrue(rlFalse1 != rlTrue1);
            Assert.IsTrue(rlTrue1 == rlTrue2);

            Assert.IsFalse(rlFalse1 & rlFalse2);
            Assert.IsFalse(rlFalse1 & rlTrue1);
            Assert.IsFalse(rlFalse1 | rlFalse2);
            Assert.IsTrue(rlFalse1 | rlTrue1);

            Assert.IsTrue(!rlFalse1);
            Assert.IsFalse(!rlTrue1);
        }

        [TestMethod]
        public void RNumberTest()
        {
            RNumber rn1 = new RNumber(1);
            RNumber rn2 = new RNumber(1.0);
            RNumber rn3 = new RNumber(2.1);

            Assert.AreEqual(RMode.Numeric, rn1.Mode);
            Assert.IsTrue(rn1 == 1);
            Assert.IsFalse(rn1 != 1);
            Assert.IsFalse(rn1 == 2);
            Assert.AreEqual(1, (double)rn1);
            Assert.AreEqual(rn1, rn2);
            Assert.AreNotEqual(rn2, rn3);
            Assert.IsTrue(rn1 != rn3);

            Assert.AreEqual(1, rn1 * rn2);
            Assert.AreEqual(3.1, rn2 + rn3);
            Assert.AreEqual(1.1, rn3 - rn1);
            Assert.AreEqual(1.05, rn3 / 2);
        }

        [TestMethod]
        public void RIntegerTest()
        {
            RInteger rn1 = new RInteger(1);
            RInteger rn2 = new RInteger(1);
            RInteger rn3 = new RInteger(2);

            Assert.AreEqual(RMode.Numeric, rn1.Mode);
            Assert.IsTrue(rn1 == 1);
            Assert.IsFalse(rn1 != 1);
            Assert.IsFalse(rn1 == 2);
            Assert.AreEqual(1, (double)rn1);
            Assert.AreEqual(rn1, rn2);
            Assert.AreNotEqual(rn2, rn3);
            Assert.IsTrue(rn1 != rn3);
        }

        [TestMethod]
        public void RVectorTest()
        {
            var rv = new RVector<RLogical>(RMode.Logical, 2);
            Assert.AreEqual(2, rv.Length);
            Assert.AreEqual(RMode.Logical, rv.Mode);
            Assert.IsFalse(rv.IsBoolean);

            rv[0] = new RLogical(false);
            rv[1] = new RLogical(true);

            Assert.AreEqual(RLogical.FALSE, rv[0]);
            Assert.AreEqual(RLogical.TRUE, rv[1]);

            rv = new RVector<RLogical>(RMode.Logical, 1);
            Assert.IsTrue(rv.IsBoolean);
        }

        [TestMethod]
        public void RListTest()
        {
            RList rl = new RList();

            Assert.AreEqual(RMode.List, rl.Mode);
            Assert.AreEqual(0, rl.Count);

            var e = rl.GetEnumerator();
            Assert.IsNotNull(e);

            Assert.IsFalse(e.MoveNext());

            RObject rv = new RVector<RNumber>(RMode.Numeric, 1);
            var rs = new RString("abc");

            rl.Add(rs, rv);

            Assert.AreEqual(1, rl.Count);

            var e1 = rl.Keys.GetEnumerator();
            e1.MoveNext();
            Assert.AreEqual(rs, e1.Current);
            Assert.AreEqual(new RString("abc"), e1.Current);

            var e2 = rl.Values.GetEnumerator();
            e2.MoveNext();
            Assert.AreEqual(rv, e2.Current);

            Assert.IsTrue(rl.ContainsKey(rs));
            Assert.IsTrue(rl.ContainsKey(new RString("abc")));

            Assert.IsTrue(rl.Contains(new KeyValuePair<RString, RObject>(rs, rv)));

            var arr = new KeyValuePair<RString, RObject>[2];
            rl.CopyTo(arr, 1);

            Assert.AreEqual(rs, arr[1].Key);
            Assert.AreEqual(rv, arr[1].Value);

            Assert.AreEqual(rv, rl[rs]);
            Assert.IsFalse(rl.IsReadOnly);

            RObject u;
            Assert.IsTrue(rl.TryGetValue(rs, out u));

            var en = rl.GetEnumerator();
            Assert.IsNotNull(en);
            Assert.IsTrue(en.MoveNext());
            Assert.AreEqual(rs, en.Current.Key);
            Assert.AreEqual(rv, en.Current.Value);
            Assert.IsFalse(en.MoveNext());

            IEnumerator<RObject> en1 = ((IEnumerable<RObject>)rl).GetEnumerator();
            Assert.IsNotNull(en1);
            Assert.IsTrue(en1.MoveNext());
            Assert.AreEqual(rv, en1.Current);
            Assert.IsFalse(en1.MoveNext());

            IEnumerator<KeyValuePair<RString, RObject>> en2 = ((IEnumerable<KeyValuePair<RString, RObject>>)rl).GetEnumerator();
            Assert.IsNotNull(en2);
            Assert.IsTrue(en2.MoveNext());
            Assert.AreEqual(rs, en2.Current.Key);
            Assert.AreEqual(rv, en2.Current.Value);
            Assert.IsFalse(en2.MoveNext());

            IEnumerator en3 = ((IEnumerable)rl).GetEnumerator();
            Assert.IsNotNull(en3);
            Assert.IsTrue(en3.MoveNext());
            Assert.IsFalse(en3.MoveNext());

            Assert.IsTrue(rl.Remove(rs));
            Assert.AreEqual(0, rl.Count);
            Assert.IsFalse(rl.ContainsKey(rs));

            rl.Add(new KeyValuePair<RString, RObject>(new RString("x"), new RLogical(true)));
            Assert.AreEqual(1, rl.Length);
            Assert.AreEqual(1, rl.Count);
            rl.Clear();
            Assert.AreEqual(0, rl.Length);
            Assert.AreEqual(0, rl.Count);

            Assert.IsFalse(rl.TryGetValue(rs, out u));
            Assert.IsNull(u);
        }
    }
}
