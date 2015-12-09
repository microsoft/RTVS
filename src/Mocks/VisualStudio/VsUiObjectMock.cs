using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public static class VsUiObjectMock {
        public static IVsUIObject Create() {
            object outValue = null;
            IVsUIObject obj = Substitute.For<IVsUIObject>();
            obj.get_Data(out outValue).ReturnsForAnyArgs(x => {
                x[0] = Resources.SampleImage;
                return VSConstants.S_OK;
            });
            return obj;
        }
    }
}
