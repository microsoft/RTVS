using System;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.R.Package.Interop
{
    [Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IOleServiceProvider
    {
        int QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject);
    }
}
