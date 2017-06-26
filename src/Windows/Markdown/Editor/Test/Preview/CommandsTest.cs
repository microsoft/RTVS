// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;

namespace Microsoft.Markdown.Editor.Test.Preview {
    [ExcludeFromCodeCoverage]
    [Category.Md.Preview]
    public sealed class CommandsTest {
        private readonly ICoreShell _shell;
        private readonly IRMarkdownEditorSettings _settings;

        public CommandsTest() {
            SetupSettings(out ICoreShell shell, out IRMarkdownEditorSettings settings);
            _shell = shell;
            _settings = settings;
        }

        public void AutomaticSync() {
            var tv = Substitute.For<ITextView>();

            var command = new AutomaticSyncCommand(tv, _shell.Services);
            command.CommandIds.Count.Should().Be(1);
            command.CommandIds[0].Group.Should().Be(MdPackageCommandId.MdCmdSetGuid);
            command.CommandIds[0].Id.Should().Be(MdPackageCommandId.icmdAutomaticSync);

            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.SupportedAndEnabled);
            _settings.AutomaticSync.Returns(true);
            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.SupportedAndEnabled | CommandStatus.Latched);

            var o = new object();
            command.Invoke();
            _settings.AutomaticSync.Should().BeFalse();

            command.Invoke();
            _settings.AutomaticSync.Should().BeTrue();
        }

        [Test]
        public void RunCurrentChunk() {
            var tb = new TextBufferMock(string.Empty, "inert");
            var tv = new TextViewMock(tb);

            var command = new RunCurrentChunkCommand(tv, _shell.Services);
            command.CommandIds.Count.Should().Be(1);
            command.CommandIds[0].Group.Should().Be(MdPackageCommandId.MdCmdSetGuid);
            command.CommandIds[0].Id.Should().Be(MdPackageCommandId.icmdRunCurrentChunk);
            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.Invisible);

            SetupTextBuffers(out ITextBuffer tbMarkdown, out ITextView textView);
            command = new RunCurrentChunkCommand(textView, _shell.Services);
            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.Invisible);

            EnableRCode(tbMarkdown, command, _settings);

            InvokeCommand(textView, command, out IMarkdownPreview preview);
            preview.Received(1).RunCurrentChunkAsync();
        }

        [Test]
        public void RunChunksAbove() {
            var tb = new TextBufferMock(string.Empty, "inert");
            var tv = new TextViewMock(tb);

            var command = new RunAllChunksAboveCommand(tv, _shell.Services);
            command.CommandIds.Count.Should().Be(1);
            command.CommandIds[0].Group.Should().Be(MdPackageCommandId.MdCmdSetGuid);
            command.CommandIds[0].Id.Should().Be(MdPackageCommandId.icmdRunAllChunksAbove);
            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.Invisible);

            SetupTextBuffers(out ITextBuffer tbMarkdown, out ITextView textView);
            command = new RunAllChunksAboveCommand(textView, _shell.Services);
            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.Invisible);

            EnableRCode(tbMarkdown, command, _settings);

            InvokeCommand(textView, command, out IMarkdownPreview preview);
            preview.Received(1).RunAllChunksAboveAsync();
        }

        private static void SetupSettings(out ICoreShell shell, out IRMarkdownEditorSettings settings) {
            var testShell = TestCoreShell.CreateSubstitute();
            settings = testShell.SetupSettingsSubstitute();
            shell = testShell;
        }

        private static void SetupTextBuffers(out ITextBuffer tbMarkdown, out ITextView textView) {
            var tbProjection = new TextBufferMock(string.Empty, MdProjectionContentTypeDefinition.ContentType);
            tbMarkdown = new TextBufferMock(string.Empty, MdContentTypeDefinition.ContentType);
            textView = new TextViewMock(new[] { tbProjection, tbMarkdown }, 0);
        }

        private void EnableRCode(ITextBuffer tbMarkdown, ICommand command, IRMarkdownEditorSettings settings) {
            var clh = Substitute.For<IContainedLanguageHandler>();
            clh.GetCodeBlockOfLocation(Arg.Any<int>()).Returns(new TextRange(0, 1));
            tbMarkdown.AddService(clh);

            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.SupportedAndEnabled);

            settings.AutomaticSync.Returns(true);
            command.Status(Guid.Empty, 0).Should().Be(CommandStatus.Supported);
        }

        private void InvokeCommand(ITextView textView, ICommand command, out IMarkdownPreview preview) {
            preview = Substitute.For<IMarkdownPreview>();
            textView.AddService(preview);

            var o = new object();
            command.Invoke(Guid.Empty, 0, null, ref o);
        }
    }
}
