// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Preview.Margin {
    // Based on https://github.com/madskristensen/MarkdownEditor/blob/master/src/Margin/BrowserMarginProvider.cs

    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name("MarginRightFactory")]
    [Order(After = PredefinedMarginNames.RightControl)]
    [MarginContainer(PredefinedMarginNames.Right)]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)] // This is to prevent the margin from loading in the diff view
    public class BrowserMarginRightProvider : IWpfTextViewMarginProvider {
        private readonly IServiceContainer _services;
        private readonly ITextDocumentFactoryService _tdfs;
        private readonly IRMarkdownEditorSettings _settings;

        [ImportingConstructor]
        public BrowserMarginRightProvider(ICoreShell coreShell) {
            _services = coreShell.Services;
            _tdfs = coreShell.Services.GetService<ITextDocumentFactoryService>();
            _settings = coreShell.Services.GetService<IRMarkdownEditorSettings>();
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer) {
            if (_settings.EnablePreview && _settings.PreviewPosition == RMarkdownPreviewPosition.Right) {
                var tv = wpfTextViewHost.TextView;
                if (_tdfs.TryGetTextDocument(tv.TextDataModel.DocumentBuffer, out ITextDocument document)) {
                    return tv.Properties.GetOrCreateSingletonProperty(() => new PreviewMargin(tv, document, _services));
                }
            }
            return null;
        }

        [Export(typeof(IWpfTextViewMarginProvider))]
        [Name("MarginBottomFactory")]
        [Order(After = PredefinedMarginNames.BottomControl)]
        [MarginContainer(PredefinedMarginNames.Bottom)]
        [ContentType(MdContentTypeDefinition.ContentType)]
        [TextViewRole(PredefinedTextViewRoles
            .Debuggable)] // This is to prevent the margin from loading in the diff view
        public class BrowserMarginBottomProvider : IWpfTextViewMarginProvider {
            private readonly IServiceContainer _services;
            private readonly ITextDocumentFactoryService _tdfs;
            private readonly IRMarkdownEditorSettings _settings;

            [ImportingConstructor]
            public BrowserMarginBottomProvider(ICoreShell coreShell) {
                _services = coreShell.Services;
                _tdfs = coreShell.Services.GetService<ITextDocumentFactoryService>();
                _settings = coreShell.Services.GetService<IRMarkdownEditorSettings>();
            }


            public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost,
                IWpfTextViewMargin marginContainer) {
                if (_settings.EnablePreview && _settings.PreviewPosition == RMarkdownPreviewPosition.Below) {
                    var tv = wpfTextViewHost.TextView;
                    if (_tdfs.TryGetTextDocument(tv.TextDataModel.DocumentBuffer, out ITextDocument document)) {
                        return tv.Properties.GetOrCreateSingletonProperty(() => new PreviewMargin(tv, document, _services));
                    }
                }
                return null;
            }
        }
    }
}