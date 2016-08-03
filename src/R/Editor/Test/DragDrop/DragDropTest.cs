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
using Microsoft.R.Host.Client.Extensions;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;
using static System.FormattableString;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.DragDrop]
    public class DragDropTest {
        private readonly EditorTestFilesFixture _files;

        public DragDropTest(EditorTestFilesFixture files) {
            _files = files;
        }

        [CompositeTest]
        [InlineData(new string[0], new string[0], "", "")]
        [InlineData(new string[] { "", "Text" }, new string[0], "", "")]
        [InlineData(new string[] { DataObjectFormats.VSProjectItems, "Text" }, new string[] { @"c:\foo\file.r"}, @"c:\", "\r\nsource('~/foo/file.r')\r\n")]
        [InlineData(new string[] { DataObjectFormats.VSProjectItems, "Text" }, new string[] { @"c:\foo\file.rmd" }, @"c:\", "'~/foo/file.rmd'")]
        [InlineData(new string[] { DataObjectFormats.VSProjectItems, "Text" }, 
                    new string[] { @"c:\foo\file1.r", @"c:\foo\file2.r" }, @"c:\", 
                    "\r\nsource('~/foo/file1.r')\r\nsource('~/foo/file2.r')\r\n")]
        public void RDataObject(string[] formats, string[] files, string projectFolder, string expected) {
            var data = Substitute.For<IDataObject>();

            data.GetFormats().Returns(formats);
            data.GetDataPresent(Arg.Any<string>()).Returns(true);
            data.GetData(DataObjectFormats.VSProjectItems).Returns(MakeStream(files));

            data.GetPlainText(projectFolder, DragDropKeyStates.None).Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("query.sql")]
        public void RSqlDataObject(string fileName) {
            var data = Substitute.For<IDataObject>();
            var fullPath = Path.Combine(_files.DestinationPath, fileName);

            data.GetFormats().Returns(new string[] { DataObjectFormats.VSProjectItems });
            data.GetDataPresent(DataObjectFormats.VSProjectItems).Returns(true);
            data.GetData(DataObjectFormats.VSProjectItems).Returns(MakeStream(new string[] { fullPath }));

            string content = File.ReadAllText(fullPath).Trim();
            data.GetPlainText(null, DragDropKeyStates.ControlKey).Should().Be('\'' + content + '\'');

            var rp = fullPath.MakeRRelativePath(_files.DestinationPath);
            data.GetData(DataObjectFormats.VSProjectItems).Returns(MakeStream(new string[] { fullPath }));
            data.GetPlainText(_files.DestinationPath, DragDropKeyStates.None)
                .Should().Be(
                    Invariant($@"iconv(paste(readLines('{rp}', encoding = 'UTF-8', warn = FALSE), collapse='\n'), from = 'UTF-8', to = 'ASCII', sub = '')")
                );
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
