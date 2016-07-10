// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Windows;
using FluentAssertions;
using Microsoft.Languages.Editor.DragDrop;
using Microsoft.R.Editor.DragDrop;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;
using static System.FormattableString;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.DragDrop]
    public class DragDropTest {
        [CompositeTest]
        [InlineData(new string[0], new string[0], "", "")]
        [InlineData(new string[] { "", "Text" }, new string[0], "", "")]
        [InlineData(new string[] { DataObjectFormats.VSProjectItems, "Text" }, new string[] { @"c:\foo\file.r"}, @"c:\", "source('~/foo/file.r')\r\n")]
        [InlineData(new string[] { DataObjectFormats.VSProjectItems, "Text" }, new string[] { @"c:\foo\file.rmd" }, @"c:\", "'~/foo/file.rmd'")]
        [InlineData(new string[] { DataObjectFormats.VSProjectItems, "Text" }, 
                    new string[] { @"c:\foo\file1.r", @"c:\foo\file2.r" }, @"c:\", 
                    "source('~/foo/file1.r')\r\nsource('~/foo/file2.r')\r\n")]
        public void RDataObject(string[] formats, string[] files, string projectFolder, string expected) {
            var data = Substitute.For<IDataObject>();

            data.GetFormats().Returns(formats);
            data.GetDataPresent(Arg.Any<string>()).Returns(true);
            data.GetData(DataObjectFormats.VSProjectItems).Returns(MakeStream(files));

            data.GetPlainText(projectFolder).Should().Be(expected);
        }

        //[CompositeTest]
        [InlineData("", 0, "")]
        [InlineData("x", 1, "x")]
        [InlineData("x(", 1, "x")]
        [InlineData("x(", 2, "")]
        [InlineData("`a`", 3, "`a`")]
        [InlineData("abc$def@", 8, "abc$def@")]
        [InlineData("abc$def@", 4, "abc$")]
        [InlineData("abc", 3, "abc")]
        [InlineData("`a bc`@`d ef `$", 7, "`a bc`@")]
        [InlineData("`a bc`@`d ef `", 14, "`a bc`@`d ef `")]
        [InlineData("`a bc` `d ef `$", 15, "`d ef `$")]
        [InlineData("x$+y$", 5, "y$")]
        [InlineData("x$+`y y`$", 9, "`y y`$")]
        [InlineData("a::b$`z`", 8, "a::b$`z`")]
        public void RDropHandler(string content, int caretPosition, string expected) {
        }

        private MemoryStream MakeStream(string[] files) {
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms, Encoding.Unicode);
            // DROPFILES
            w.Write(20); // offset
            w.Write(0);  // X
            w.Write(0);  // Y
            w.Write(0);  // NC
            w.Write(1);  // Unicode
            foreach (var f in files) {
                var s = Invariant($"{Guid.Empty.ToString()}|file.rproj|{f}");
                foreach(var ch in s) {
                    w.Write(ch);
                }
                w.Write('\0');
            }
            w.Write(0);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
