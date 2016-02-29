// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks
{
    [ExcludeFromCodeCoverage]
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
