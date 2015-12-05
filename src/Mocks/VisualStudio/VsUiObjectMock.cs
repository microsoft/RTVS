using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsUiObjectMock : IVsUIObject {
        public int Equals(IVsUIObject pOtherObject, out bool pfAreEqual) {
            pfAreEqual = pOtherObject == this;
            return VSConstants.S_OK;
        }

        public int get_Data(out object pVar) {
            pVar = Resources.SampleImage;
            return VSConstants.S_OK;
        }

        public int get_Format(out uint pdwDataFormat) {
            throw new NotImplementedException();
        }

        public int get_Type(out string pTypeName) {
            throw new NotImplementedException();
        }
    }
}
