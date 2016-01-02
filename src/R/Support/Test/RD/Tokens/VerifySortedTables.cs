using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Support.RD.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.RD.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class VerifySortedRdTables : TokenizeTestBase<RdToken, RdTokenType> {
        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void VerifySortedRdBlockKeywords() {
            string[] array = new List<string>(RdBlockContentType._rKeywords).ToArray();
            Array.Sort(array);

            for (int i = 0; i < array.Length; i++) {
                Assert.AreEqual(RdBlockContentType._rKeywords[i], array[i]);
            }
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void VerifySortedRdVerbatimKeywords() {
            string[] array = new List<string>(RdBlockContentType._verbatimKeywords).ToArray();
            Array.Sort(array);

            for (int i = 0; i < array.Length; i++) {
                Assert.AreEqual(RdBlockContentType._verbatimKeywords[i], array[i]);
            }
        }
    }
}
