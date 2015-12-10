using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class VerifySortedTables : TokenizeTestBase<RToken, RTokenType> {
        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void VerifySortedRKeywords() {
            string[] array = new List<string>(Keywords.KeywordList).ToArray();
            Array.Sort(array);

            for (int i = 0; i < array.Length; i++) {
                Assert.AreEqual(Keywords.KeywordList[i], array[i]);
            }
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void VerifySorted2CharOperators() {
            string[] array = new List<string>(Operators._twoChars).ToArray();
            Array.Sort(array);

            for (int i = 0; i < array.Length; i++) {
                Assert.AreEqual(Operators._twoChars[i], array[i]);
            }
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void VerifySorted3CharOperators() {
            string[] array = new List<string>(Operators._threeChars).ToArray();
            Array.Sort(array);

            for (int i = 0; i < array.Length; i++) {
                Assert.AreEqual(Operators._threeChars[i], array[i]);
            }
        }
    }
}
