using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Utilities {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class CollectionUtilitiesTest {
        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void InplaceUpdateAddTest() {
            List<IntegerWrap> source = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(3) };
            List<IntegerWrap> update = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);

            Assert.AreEqual(update.Count, source.Count);
            for (int i = 0; i < update.Count; i++) {
                Assert.IsTrue(IntegerComparer(source[i], update[i]));
            }
            Assert.IsTrue(source[0].Updated);
            Assert.IsFalse(source[1].Updated);
            Assert.IsTrue(source[2].Updated);
            Assert.IsFalse(source[3].Updated);
        }

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void InplaceUpdateRemoveTest() {
            List<IntegerWrap> source = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };
            List<IntegerWrap> update = new List<IntegerWrap>() { new IntegerWrap(2), new IntegerWrap(4) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);

            Assert.AreEqual(update.Count, source.Count);
            for (int i = 0; i < update.Count; i++) {
                Assert.IsTrue(IntegerComparer(source[i], update[i]));
            }
            Assert.IsTrue(source[0].Updated);
            Assert.IsTrue(source[1].Updated);
        }

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void InplaceUpdateMixedTest() {
            List<IntegerWrap> source = new List<IntegerWrap>() { new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };
            List<IntegerWrap> update = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);

            Assert.AreEqual(update.Count, source.Count);
            for (int i = 0; i < update.Count; i++) {
                Assert.IsTrue(IntegerComparer(source[i], update[i]));
            }
            Assert.IsFalse(source[0].Updated);
            Assert.IsTrue(source[1].Updated);
            Assert.IsTrue(source[1].Updated);
        }

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void InplaceUpdateRemoveAllTest() {
            List<IntegerWrap> source = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3) };
            List<IntegerWrap> update = new List<IntegerWrap>() { };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);

            Assert.AreEqual(update.Count, source.Count);
            for (int i = 0; i < update.Count; i++) {
                Assert.IsTrue(IntegerComparer(source[i], update[i]));
            }
        }

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void InplaceUpdateAddToEmptyTest() {
            List<IntegerWrap> source = new List<IntegerWrap>() { };
            List<IntegerWrap> update = new List<IntegerWrap>() { new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);

            Assert.AreEqual(update.Count, source.Count);
            for (int i = 0; i < update.Count; i++) {
                Assert.IsTrue(IntegerComparer(source[i], update[i]));
            }
            Assert.IsFalse(source[0].Updated);
            Assert.IsFalse(source[1].Updated);
            Assert.IsFalse(source[2].Updated);
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
                return string.Format("{0} {1}", Value, Updated);
            }
        }
    }
}
