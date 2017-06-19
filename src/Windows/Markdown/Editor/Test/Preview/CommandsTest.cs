// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Preview;
using Microsoft.Markdown.Editor.Preview.Commands;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;

namespace Microsoft.Markdown.Editor.Test.Preview {
    [ExcludeFromCodeCoverage]
    [Category.Md.Preview]
    public class CommandsTest {
        public void AutomaticSync() {
            var tv = Substitute.For<ITextView>();
            var settings = Substitute.For<IRMarkdownEditorSettings>();
            var shell = TestCoreShell.CreateSubstitute();
            shell.ServiceManager.AddService(settings);

            var command = new AutomaticSyncCommand(tv, shell.Services);
            command.CommandIds.Count.Should().Be(1);
            command.CommandIds[0].Group.Should().Be(MdPackageCommandId.MdCmdSetGuid);
            command.CommandIds[0].Id.Should().Be(MdPackageCommandId.icmdAutomaticSync);

            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.SupportedAndEnabled);
            settings.AutomaticSync.Returns(true);
            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.SupportedAndEnabled | CommandStatus.Latched);

            var o = new object();
            command.Invoke(Guid.Empty, 0, null, ref o);
            settings.AutomaticSync.Should().BeFalse();

            command.Invoke(Guid.Empty, 0, null, ref o);
            settings.AutomaticSync.Should().BeTrue();
        }

        [Test]
        public void RunCurrentChunk() {
            var settings = Substitute.For<IRMarkdownEditorSettings>();
            var shell = TestCoreShell.CreateSubstitute();
            shell.ServiceManager.AddService(settings);

            var tb = new TextBufferMock(string.Empty, "inert");
            var tv = new TextViewMock(tb);

            var command = new RunCurrentChunkCommand(tv, shell.Services);
            command.CommandIds.Count.Should().Be(1);
            command.CommandIds[0].Group.Should().Be(MdPackageCommandId.MdCmdSetGuid);
            command.CommandIds[0].Id.Should().Be(MdPackageCommandId.icmdRunCurrentChunk);
            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.Invisible);

            var tb1 = new TextBufferMock(string.Empty, MdProjectionContentTypeDefinition.ContentType);
            var tb2 = new TextBufferMock(string.Empty, MdContentTypeDefinition.ContentType);
            tv = new TextViewMock(new [] {tb1, tb2}, 0);
            command = new RunCurrentChunkCommand(tv, shell.Services);
            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.Invisible);

            var clh = Substitute.For<IContainedLanguageHandler>();
            clh.GetCodeBlockOfLocation(Arg.Any<int>()).Returns(new TextRange(0, 1));
            tb.AddService(clh);

            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.SupportedAndEnabled);

            settings.AutomaticSync.Returns(true);
            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.Supported);

            var preview = Substitute.For<IMarkdownPreview>();
            tv.AddService(preview);

            var o = new object();
            command.Invoke(Guid.Empty, 0, null, ref o);
            preview.Received(1).RunCurrentChunkAsync();
        }
    }
}
