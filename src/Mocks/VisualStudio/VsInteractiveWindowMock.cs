using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsInteractiveWindowMock : IVsInteractiveWindow {

        public IInteractiveWindow InteractiveWindow { get; private set; } = new InteractiveWindowMock();

        public void SetLanguage(Guid languageServiceGuid, IContentType contentType) {
        }

        public void Show(bool focus) {
        }
    }
}
