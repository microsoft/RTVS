// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class HtmlParser_StringClosureTest {
        [Test]
        [Category.Html]
        public void HtmlParser_StringClosure1() {
            string text = "\"=\"";
            StringClosure closure = CreateStringClosure(text);

            int actual = closure.GetStringClosureLocation(text.Length);
            Assert.Equal(3, actual);
        }

        [Test]
        [Category.Html]
        public void HtmlParser_StringClosure2() {
            string text = "\"dir=\"";
            StringClosure closure = CreateStringClosure(text);

            int actual = closure.GetStringClosureLocation(text.Length);
            Assert.Equal(text.Length, actual);
        }

        [Test]
        [Category.Html]
        public void HtmlParser_StringClosure3() {
            string text = "\" />";
            StringClosure closure = CreateStringClosure(text);

            int actual = closure.GetStringClosureLocation(text.Length);
            Assert.Equal(1, actual);
        }

        [Test]
        [Category.Html]
        public void HtmlParser_StringClosure4() {
            string text = "\"foo </div>";
            StringClosure closure = CreateStringClosure(text);

            int actual = closure.GetStringClosureLocation(text.Length);
            Assert.Equal(5, actual);
        }

        [Test]
        [Category.Html]
        public void HtmlParser_StringClosure5() {
            string text = "\"foo < bar\"";
            StringClosure closure = CreateStringClosure(text);

            int actual = closure.GetStringClosureLocation(text.Length);
            Assert.Equal(text.Length, actual);
        }

        [Test]
        [Category.Html]
        public void HtmlParser_StringClosure_EndsWithEquals_OddQuotes() {
            string text = "\"a b=\" c=\"\"";
            StringClosure closure = CreateStringClosure(text);

            int actual = closure.GetStringClosureLocation(text.Length);
            Assert.Equal(6, actual);
        }

        [Test]
        [Category.Html]
        public void HtmlParser_StringClosure_EndsWithEquals_EvenQuotes() {
            string text = "\"a b=\"c\"";
            StringClosure closure = CreateStringClosure(text);

            int actual = closure.GetStringClosureLocation(text.Length);
            Assert.Equal(2, actual);
        }

        [Test]
        [Category.Html]
        public void HtmlParser_StringClosure_EndsWithEquals_NewLine() {
            string text = "\"a\r\n b=\"c\"";
            StringClosure closure = CreateStringClosure(text);

            int actual = closure.GetStringClosureLocation(text.Length);
            Assert.Equal(2, actual);
        }

        private StringClosure CreateStringClosure(string text) {
            HtmlCharStream stream = new HtmlCharStream(text);
            StringClosure closure = new StringClosure(stream);
            return closure;
        }
    }
}
