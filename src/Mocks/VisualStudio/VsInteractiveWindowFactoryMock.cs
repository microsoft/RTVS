using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {

    [ExcludeFromCodeCoverage]
    [Export(typeof(IVsInteractiveWindowFactory))]
    public sealed class VsInteractiveWindowFactoryMock : IVsInteractiveWindowFactory {
        private Lazy<VsInteractiveWindowMock> _interactiveWindow = new Lazy<VsInteractiveWindowMock>(() => new VsInteractiveWindowMock());
        public IVsInteractiveWindow Create(Guid providerId, int instanceId, string title, IInteractiveEvaluator evaluator, __VSCREATETOOLWIN creationFlags = 0) {
            return _interactiveWindow.Value;
        }
    }
}
