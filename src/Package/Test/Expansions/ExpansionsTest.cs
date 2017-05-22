// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.R.Package.Expansions;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Package {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]   // required for tests using R Host 
    public class ExpansionsTest {
        private readonly IVsExpansionManager _expansionManager;
        private readonly IExpansionsCache _cache;
        private readonly IServiceContainer _services;

        public ExpansionsTest(IServiceContainer services) {
            _services = services;
            _expansionManager = Substitute.For<IVsExpansionManager>();

            _cache = Substitute.For<IExpansionsCache>();
            _cache.GetExpansion("if").Returns(new VsExpansion {
                description = "if statement",
                path = "path",
                shortcut = "if",
                title = "if statement"
            });
        }

        [Test]
       public void ExpansionClientTest() {
            var textBuffer = new TextBufferMock("if", RContentTypeDefinition.ContentType);
            var textView = new TextViewMock(textBuffer);
            var client = new ExpansionClient(textView, textBuffer, _expansionManager, _cache, _services);

            client.IsEditingExpansion().Should().BeFalse();
            client.IsCaretInsideSnippetFields().Should().BeFalse();

            _expansionManager.InvokeInsertionUI(null, null, Guid.Empty, new string[0], 0, 0, new string[0], 0, 0, string.Empty, string.Empty)
                .ReturnsForAnyArgs(VSConstants.S_OK);

            client.InvokeInsertionUI((int)VSConstants.VSStd2KCmdID.INSERTSNIPPET).Should().Be(VSConstants.S_OK);

            textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, 2));

            bool inserted;
            client.StartSnippetInsertion(out inserted);

            inserted.Should().BeTrue();
            client.IsEditingExpansion().Should().BeTrue();

            client.EndExpansion();
            client.IsEditingExpansion().Should().BeFalse();

            client.OnItemChosen("if", "path");
            client.IsEditingExpansion().Should().BeTrue();

            client.EndExpansion();
            client.IsEditingExpansion().Should().BeFalse();
            client.IsCaretInsideSnippetFields().Should().BeFalse();
        }

        [Test]
        public void ExpansionControllerTest() {
            var textBuffer = new TextBufferMock("if", RContentTypeDefinition.ContentType);
            var textView = new TextViewMock(textBuffer);
            var o = new object();

            var controller = new ExpansionsController(textView, textBuffer, _expansionManager, _cache, _services);
            controller.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.INSERTSNIPPET).Should().Be(CommandStatus.SupportedAndEnabled);
            controller.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.SURROUNDWITH).Should().Be(CommandStatus.SupportedAndEnabled);

            controller.Invoke(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.RETURN, null, ref o).Should().Be(CommandResult.NotSupported);
            controller.Invoke(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.BACKTAB, null, ref o).Should().Be(CommandResult.NotSupported);
            controller.Invoke(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.CANCEL, null, ref o).Should().Be(CommandResult.NotSupported);

            textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, 2));
            bool inserted;

            var client = controller.ExpansionClient as ExpansionClient;
            client.StartSnippetInsertion(out inserted);

            controller.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.INSERTSNIPPET).Should().Be(CommandStatus.SupportedAndEnabled);
            controller.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.SURROUNDWITH).Should().Be(CommandStatus.SupportedAndEnabled);

            client.Session.Should().NotBeNull();
            var session = client.Session;
            var mock = session as VsExpansionSessionMock;

            controller.Invoke(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.TAB, null, ref o).Should().Be(CommandResult.Executed);
            mock.ExpansionFieldIndex.Should().Be(1);

            controller.Invoke(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.BACKTAB, null, ref o).Should().Be(CommandResult.Executed);
            mock.ExpansionFieldIndex.Should().Be(0);

            client.EndExpansion();
        }
    }
}
