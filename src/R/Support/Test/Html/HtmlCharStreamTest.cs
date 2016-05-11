// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Text {
    [ExcludeFromCodeCoverage]
    public class HtmlCharStreamTest {
        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [Test]
        [Category.Html]
        public void HtmlCharStream_IsNameCharTest() {
            var stream = new HtmlCharStream(new TextStream(""));
            Assert.True(stream.IsEndOfStream());
            Assert.Equal(0, stream.Length);

            stream = new HtmlCharStream(new TextStream("<h123"));
            Assert.Equal(0, stream.Position);
            Assert.False(stream.IsEndOfStream());
            stream.Position = 5;
            Assert.True(stream.IsEndOfStream());
            stream.Position = 0;
            Assert.False(stream.IsEndOfStream());

            stream.MoveToNextChar();
            Assert.Equal(1, stream.Position);

            stream.Advance(2);
            Assert.Equal(3, stream.Position);

            stream.Advance(-2);
            Assert.Equal(1, stream.Position);

            stream.Advance(1000);
            Assert.True(stream.IsEndOfStream());

            stream.Position = 0;
            Assert.True(stream.IsAtTagDelimiter());
            Assert.Equal('<', stream.CurrentChar);
            Assert.Equal('h', stream.NextChar);

            stream.Position = 1;
            Assert.False(stream.IsAtTagDelimiter());
            Assert.True(stream.IsNameChar());
            Assert.True(HtmlCharStream.IsNameStartChar(stream.CurrentChar));

            stream.Position = 2;
            Assert.False(stream.IsAtTagDelimiter());
            Assert.True(stream.IsNameChar());
            Assert.False(HtmlCharStream.IsNameStartChar(stream.CurrentChar));
        }

        [Test]
        [Category.Html]
        public void HtmlCharStream_IsNameStartCharTest() {
            // NameStartChar ::= ":" | [A-Z] | "_" | [a-z] | 
            //                      [#xC0-#xD6] | [#xD8-#xF6] | [#xF8-#x2FF] | [#x370-#x37D] | 
            //                      [#x37F-#x1FFF] | [#x200C-#x200D] | [#x2070-#x218F] | [#x2C00-#x2FEF] | 
            //                      [#x3001-#xD7FF] | [#xF900-#xFDCF] | [#xFDF0-#xFFFD] | [#x10000-#xEFFFF]         

            for (int i = 0; i <= Int16.MaxValue; i++) {
                // We don't allow leading :

                if (i == '_')
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 'A' && i <= 'Z')
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 'a' && i <= 'z')
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 0xC0 && i <= 0xD6)
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 0xD8 && i <= 0xF6)
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 0xF8 && i <= 0x2FF)
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 0x370 && i <= 0x37D)
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 0x37F && i <= 0x1FFF)
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 0x200C && i <= 0x200D)
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 0x2070 && i <= 0x218F)
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 0x2C00 && i <= 0x2FEF)
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 0x3001 && i <= 0xD7FF)
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 0xF900 && i <= 0xFDCF)
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else if (i >= 0xFDF0 && i <= 0xFFFD)
                    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                //else if (i >= 0x10000 && i <= 0xEFFFF)
                //    Assert.True(HtmlCharStream.IsNameStartChar((char)i));
                else
                    Assert.False(HtmlCharStream.IsNameStartChar((char)i));
            }
        }
    }
}
