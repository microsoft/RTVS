// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Outlining;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Outline {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class OutlineTest {
        private readonly IExportProvider _exportProvider;
        private readonly EditorHostMethodFixture _editorHost;
        private readonly EditorAppTestFilesFixture _files;
        
        public OutlineTest(IExportProvider exportProvider, EditorHostMethodFixture editorHost, EditorAppTestFilesFixture files) {
            _exportProvider = exportProvider;
            _editorHost = editorHost;
            _files = files;
        }

        [Test]
        [Category.Interactive]
        public async Task R_OutlineToggleAll() {
            string text = _files.LoadDestinationFile("lsfit.r");
            using (var script = await _editorHost.StartScript(_exportProvider, text, "filename", RContentTypeDefinition.ContentType, null)) {
                script.DoIdle(500);

                IOutliningManagerService svc = _exportProvider.GetExportedValue<IOutliningManagerService>();
                IOutliningManager mgr = svc.GetOutliningManager(script.View);
                var snapshot = script.TextBuffer.CurrentSnapshot;

                var viewLines = script.View.TextViewLines;
                viewLines.Count.Should().Be(22);
                script.DoIdle(500);

                script.Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_ALL);
                script.DoIdle(1000);

                IEnumerable<ICollapsed> collapsed = mgr.GetCollapsedRegions(new SnapshotSpan(snapshot, new Span(0, snapshot.Length)));
                collapsed.Count().Should().Be(20);

                script.Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_ALL);
                script.DoIdle(500);

                viewLines = script.View.TextViewLines;
                viewLines.Count.Should().Be(22);

                script.Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL);
                script.DoIdle(200);
                mgr.Enabled.Should().Be(false);

                script.Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING);
                script.DoIdle(200);
                mgr.Enabled.Should().Be(true);

                script.MoveDown(9);
                script.Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_CURRENT);
                script.DoIdle(500);

                collapsed = mgr.GetCollapsedRegions(new SnapshotSpan(snapshot, new Span(0, snapshot.Length)));
                collapsed.Count().Should().Be(1);

                script.Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_CURRENT);
                script.DoIdle(200);

                collapsed = mgr.GetCollapsedRegions(new SnapshotSpan(snapshot, new Span(0, snapshot.Length)));
                collapsed.Count().Should().Be(0);
            }
        }
    }
}
