using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.AST
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RDataTypesTest : UnitTestBase
    {
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
        }
    }
}
