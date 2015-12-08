using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    [Guid("0624ED9D-7C3A-4C63-9DEC-FDD38DC3C0E1")]
    public sealed class ToolWindowPaneMock : ToolWindowPane {
        public ToolWindowPaneMock(IServiceProvider sp) : base(sp) {
        }
    }
}
