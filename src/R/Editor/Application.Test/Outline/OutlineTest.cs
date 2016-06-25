// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Outlining;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Outline {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class OutlineTest : IDisposable {
        private readonly IExportProvider _exportProvider;
        private readonly EditorHostMethodFixture _editorHost;
        private readonly EditorAppTestFilesFixture _files;
        
        public OutlineTest(REditorApplicationMefCatalogFixture catalogFixture, EditorHostMethodFixture editorHost, EditorAppTestFilesFixture files) {
            _exportProvider = catalogFixture.CreateExportProvider();
            _editorHost = editorHost;
            _files = files;
        }

        public void Dispose() {
            _exportProvider.Dispose();
        }

        [Test]
        [Category.Interactive]
        public void R_OutlineToggleAll() {
            string text = _files.LoadDestinationFile("lsfit.r");
            using (var script = _editorHost.StartScript(_exportProvider, text, RContentTypeDefinition.ContentType)) {
                script.DoIdle(500);

                IOutliningManagerService svc = EditorShell.Current.ExportProvider.GetExportedValue<IOutliningManagerService>();
                IOutliningManager mgr = svc.GetOutliningManager(EditorWindow.CoreEditor.View);
                var snapshot = EditorWindow.TextBuffer.CurrentSnapshot;

                var viewLines = EditorWindow.CoreEditor.View.TextViewLines;
                viewLines.Count.Should().Be(40);
                script.DoIdle(500);

                EditorWindow.ExecCommand(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_ALL);
                script.DoIdle(1000);

                IEnumerable<ICollapsed> collapsed = mgr.GetCollapsedRegions(new SnapshotSpan(snapshot, new Span(0, snapshot.Length)));
                collapsed.Count().Should().Be(20);

                EditorWindow.ExecCommand(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_ALL);
                script.DoIdle(500);

                viewLines = EditorWindow.CoreEditor.View.TextViewLines;
                viewLines.Count.Should().Be(40);

                EditorWindow.ExecCommand(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL);
                script.DoIdle(200);
                mgr.Enabled.Should().Be(false);

                EditorWindow.ExecCommand(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING);
                script.DoIdle(200);
                mgr.Enabled.Should().Be(true);

                script.MoveDown(9);
                EditorWindow.ExecCommand(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_CURRENT);
                script.DoIdle(500);

                collapsed = mgr.GetCollapsedRegions(new SnapshotSpan(snapshot, new Span(0, snapshot.Length)));
                collapsed.Count().Should().Be(1);

                EditorWindow.ExecCommand(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_CURRENT);
                script.DoIdle(200);

                collapsed = mgr.GetCollapsedRegions(new SnapshotSpan(snapshot, new Span(0, snapshot.Length)));
                collapsed.Count().Should().Be(0);
            }
        }
    }
}
