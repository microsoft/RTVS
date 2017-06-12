// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Margin {
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
        private readonly RMarkdownOptions _options;

        [ImportingConstructor]
        public BrowserMarginRightProvider(ICoreShell coreShell) {
            _services = coreShell.Services;
            _tdfs = coreShell.Services.GetService<ITextDocumentFactoryService>();
            _options = coreShell.Services.GetService<IREditorSettings>().MarkdownOptions;
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer) {
            if (_options.EnablePreview && _options.PreviewPosition == RMarkdownPreviewPosition.Right) {
                var tv = wpfTextViewHost.TextView;
                if (_tdfs.TryGetTextDocument(tv.TextDataModel.DocumentBuffer, out ITextDocument document)) {
                    return tv.Properties.GetOrCreateSingletonProperty(() => new BrowserMargin(tv, document, _services));
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
            private readonly RMarkdownOptions _options;

            [ImportingConstructor]
            public BrowserMarginBottomProvider(ICoreShell coreShell) {
                _services = coreShell.Services;
                _tdfs = coreShell.Services.GetService<ITextDocumentFactoryService>();
                _options = coreShell.Services.GetService<IREditorSettings>().MarkdownOptions;
            }


            public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost,
                IWpfTextViewMargin marginContainer) {
                if (_options.EnablePreview && _options.PreviewPosition == RMarkdownPreviewPosition.Below) {
                    var tv = wpfTextViewHost.TextView;
                    if (_tdfs.TryGetTextDocument(tv.TextDataModel.DocumentBuffer, out ITextDocument document)) {
                        return tv.Properties.GetOrCreateSingletonProperty(() => new BrowserMargin(tv, document, _services));
                    }
                }
                return null;
            }
        }

        [Export(typeof(IWpfTextViewMarginProvider))]
        [Name("LiveSyncFactory")]
        [Order(Before = PredefinedMarginNames.HorizontalScrollBarContainer)]
        [MarginContainer(PredefinedMarginNames.BottomRightCorner)]
        [ContentType(MdContentTypeDefinition.LanguageName)]
        [TextViewRole(PredefinedTextViewRoles.Debuggable)] // This is to prevent the margin from loading in the diff view
        public class LiveSyncMarginBottomProvider : IWpfTextViewMarginProvider {
            private readonly IServiceContainer _services;
            private readonly RMarkdownOptions _options;

            [ImportingConstructor]
            public LiveSyncMarginBottomProvider(ICoreShell coreShell) {
                _services = coreShell.Services;
                _options = coreShell.Services.GetService<IREditorSettings>().MarkdownOptions;
            }

            public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer) {
                if (_options.EnablePreview) {
                    var tv = wpfTextViewHost.TextView;
                    return tv.Properties.GetOrCreateSingletonProperty(() => new LiveSyncMargin(tv, _services));
                }
                return null;
            }
        }
    }
}