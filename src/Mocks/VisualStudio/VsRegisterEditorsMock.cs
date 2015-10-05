using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks
{
    public sealed class VsRegisterEditorsMock : IVsRegisterEditors
    {
        public int RegisterEditor(ref Guid rguidEditor, IVsEditorFactory pVsPF, out uint pdwCookie)
        {
            pdwCookie = 1;
            return VSConstants.S_OK;
        }

        public int UnregisterEditor(uint dwCookie)
        {
            return VSConstants.S_OK;
        }
    }
}
