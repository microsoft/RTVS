using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {

    [AppliesTo("RTools")]
    internal sealed class Export {

        [Export(typeof(IFileSystem))]
        private IFileSystem FileSystem { get; }

        [Export(typeof(IRSessionProvider))]
        private IRSessionProvider RSessionProvider { get; }

        [Export(typeof(IRHistoryProvider))]
        private IRHistoryProvider RHistoryProvider { get; }

        [ImportingConstructor]
        public Export(Lazy<IEditorOperationsFactoryService> editorOperationsFactoryLazy, IRtfBuilderService rtfBuilderService, ITextSearchService2 textSearchService) {
            FileSystem = new FileSystem();
            RSessionProvider = new RSessionProvider();
            RHistoryProvider = new RHistoryProvider(FileSystem, editorOperationsFactoryLazy, rtfBuilderService, textSearchService);
        }
    }
}