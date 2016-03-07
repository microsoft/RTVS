// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.R.Package.Expansions;
using Microsoft.VisualStudio.Shell.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Package {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]   // required for tests using R Host 
    public class ExpansionsTest {
        [Test]
       public void ExpansionClientTest() {
            var tb = new VsTextBufferMock("if");
            var tv = new TextViewMock(tb);
            var em = Substitute.For<IVsExpansionManager>();
            var cache = Substitute.For<IExpansionsCache>();
            var client = new ExpansionClient(tv, tb, em, cache);

            client.IsEditingExpansion().Should().BeFalse();
            client.IsCaretInsideSnippetFields().Should().BeFalse();

            em.InvokeInsertionUI(null, null, Guid.Empty, new string[0], 0, 0, new string[0], 0, 0, string.Empty, string.Empty)
                .ReturnsForAnyArgs(VSConstants.S_OK);

            client.InvokeInsertionUI((int)VSConstants.VSStd2KCmdID.INSERTSNIPPET).Should().Be(VSConstants.S_OK);

            cache.GetExpansion("if").Returns(new VsExpansion() {
                description = "if statement",
                path = "path",
                shortcut = "if",
                title = "if statement"
            });

            tv.Caret.MoveTo(new SnapshotPoint(tv.TextBuffer.CurrentSnapshot, 2));

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
        }
    }
}
