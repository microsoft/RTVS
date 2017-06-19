// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Preview.Margin {
    internal abstract class PreviewMarginProviderBase: IWpfTextViewMarginProvider {
        private readonly IServiceContainer _services;
        private readonly IRMarkdownEditorSettings _settings;
        private readonly RMarkdownPreviewPosition _position;

        protected PreviewMarginProviderBase(IServiceContainer services, RMarkdownPreviewPosition position) {
            _services = services;
            _settings = services.GetService<IRMarkdownEditorSettings>();
            _position = position;
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer) {
            if (_settings.EnablePreview && _settings.PreviewPosition == _position) {
                var tv = wpfTextViewHost.TextView;
                return tv.Properties.GetOrCreateSingletonProperty(() => new PreviewMargin(tv, _services));
            }
            return null;
        }
    }
}
