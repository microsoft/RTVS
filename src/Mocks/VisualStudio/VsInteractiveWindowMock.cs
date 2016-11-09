// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsInteractiveWindowMock : ToolWindowPane, IVsInteractiveWindow {
        private IWpfTextView _textView;

        public VsInteractiveWindowMock(IWpfTextView textView) {
            _textView = textView;
            InteractiveWindow = new InteractiveWindowMock(_textView);
        }

        public IInteractiveWindow InteractiveWindow { get; private set; }

        public void SetLanguage(Guid languageServiceGuid, IContentType contentType) {
        }

        public void Show(bool focus) {
        }
    }
}
