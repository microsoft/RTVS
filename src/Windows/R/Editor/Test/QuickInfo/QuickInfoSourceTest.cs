// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Script;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.QuickInfo;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Editor.Test.QuickInfo {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    [Collection(CollectionNames.NonParallel)]
    public class FunctionIndexTest : FunctionIndexBasedTest {
        class Session {
            public AstRoot Ast;
            public ITrackingSpan ApplicableSpan;
            public List<object> QuickInfoContent;
            public IEditorBuffer EditorBuffer;
        }

        public FunctionIndexTest(IServiceContainer services) : base(services) { }

        [CompositeTest]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CacheTest(bool cached) {
            if (!cached) {
                PackageIndex.ClearCache();
            }

            var content = @"x <- as.matrix(x)";
            var session = await TriggerSessionAsync(content, 15);
            var parametersInfo = session.Ast.GetSignatureInfoFromBuffer(session.EditorBuffer.CurrentSnapshot, 10);

            session.ApplicableSpan.Should().NotBeNull();
            session.QuickInfoContent.Should().ContainSingle()
                .Which.ToString().Should().StartWith("as.matrix(x, ...)");
        }

        [Test]
        public async Task AliasTest() {
            // 'as.Date.character' RD contains no function info for 'as.Date.character', but the one for 'as.Date'
            // and as.Date.character appears as alias. We verify as.Date.character is shown in the signature info.
            var content = @"x <- as.Date.character(x)";

            var session = await TriggerSessionAsync(content, 23);
            var parametersInfo = session.Ast.GetSignatureInfoFromBuffer(session.EditorBuffer.CurrentSnapshot, 23);

            session.ApplicableSpan.Should().NotBeNull();
            session.QuickInfoContent.Should().ContainSingle()
                .Which.ToString().Should().StartWith("as.Date.character(x, ...)");
        }


        [Test]
        public async Task NonUniqueNameTest() {
            var content = @"x <- select()";

            using (new RHostScript(Services)) {
                var session = await TriggerSessionAsync(content, 12);

                session.ApplicableSpan.Should().BeNull();
                session.QuickInfoContent.Should().BeEmpty();

                await EnsurePackageInstalled("MASS");
                await EnsurePackageInstalled("dplyr");

                await Workflow.RSession.ExecuteAsync("library(MASS)");
                EventsPump.DoEvents(500);
                session = await TriggerSessionAsync(content, 12);

                session.ApplicableSpan.Should().NotBeNull();
                session.QuickInfoContent.Should().ContainSingle().Which.ToString().Should().StartWith("select(formula");

                await Workflow.RSession.ExecuteAsync("library(dplyr)");
                EventsPump.DoEvents(500);
                session = await TriggerSessionAsync(content, 12);

                session.ApplicableSpan.Should().NotBeNull();
                session.QuickInfoContent.Should().ContainSingle().Which.ToString().Should().StartWith("select(.data");
            }
        }

        [Test]
        public async Task LoadUnloadPackageTest() {
            var content = @"do()";

            using (new RHostScript(Services)) {
                await EnsurePackageInstalled("dplyr");

                var session = await TriggerSessionAsync(content, 3);

                session.ApplicableSpan.Should().BeNull();
                session.QuickInfoContent.Should().BeEmpty();

                await Workflow.RSession.ExecuteAsync("library(dplyr)");
                session = await TriggerSessionAsync(content, 3);

                session.ApplicableSpan.Should().NotBeNull();
                session.QuickInfoContent.Should().ContainSingle().Which.ToString().Should().StartWith("do(.data");

                await Workflow.RSession.ExecuteAsync("detach(\"package:dplyr\", unload = TRUE)");
                EventsPump.DoEvents(1000);

                session = await TriggerSessionAsync(content, 3);
                session.QuickInfoContent.Should().BeEmpty();
            }
        }

        private async Task EnsurePackageInstalled(string name) {
            var packages = await Workflow.Packages.GetInstalledPackagesAsync();
            if (!packages.Any(p => p.Package.EqualsOrdinal(name))) {
                await Workflow.Packages.InstallPackageAsync(name, null);
            }
        }

        private async Task<Session> TriggerSessionAsync(string content, int position) {
            var s = new Session {
                Ast = RParser.Parse(content),
                EditorBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType).ToEditorBuffer()
            };

            var tree = new EditorTreeMock(s.EditorBuffer, s.Ast);
            var document = new EditorDocumentMock(tree);
            var textBuffer = s.EditorBuffer.As<ITextBuffer>();
            var quickInfoSource = new QuickInfoSource(textBuffer, Services);
            var quickInfoSession = new QuickInfoSessionMock(textBuffer, position);
            s.QuickInfoContent = new List<object>();

            quickInfoSession.TriggerPoint = new SnapshotPoint(s.EditorBuffer.TextSnapshot(), position);
            s.ApplicableSpan = await quickInfoSource.AugmentQuickInfoSessionAsync(s.Ast, textBuffer, position, quickInfoSession, s.QuickInfoContent);

            return s;
        }
    }
}
