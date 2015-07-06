using System;
using System.Collections.Generic;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class VerifySortedTables : TokenizeTestBase
    {
        [TestMethod]
        public void VerifySorted2CharOperators()
        {
            string[] array = new List<string>(Operators._twoChars).ToArray();
            Array.Sort(array);

            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(Operators._twoChars[i], array[i]);
            }
        }

        [TestMethod]
        public void VerifySorted3CharOperators()
        {
            string[] array = new List<string>(Operators._threeChars).ToArray();
            Array.Sort(array);

            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(Operators._threeChars[i], array[i]);
            }
        }
    }
}
