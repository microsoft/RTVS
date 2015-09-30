using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks
{
    public sealed class VsRegisterProjectGeneratorsMock : IVsRegisterProjectGenerators
    {
        public void RegisterProjectGenerator([In] ref Guid rguidProjGenerator, [In, MarshalAs(UnmanagedType.Interface)] IVsProjectGenerator pProjectGenerator, out uint pdwCookie)
        {
            pdwCookie = 1;
        }

        public void UnregisterProjectGenerator([In] uint dwCookie)
        {
        }
    }
}
