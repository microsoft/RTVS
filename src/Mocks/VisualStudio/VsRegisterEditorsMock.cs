// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.Shell.Mocks
{
    [ExcludeFromCodeCoverage]
    public static class VsRegisterEditorsMock 
    {
        public static IVsRegisterEditors Create() {
            IVsRegisterEditors re = Substitute.For<IVsRegisterEditors>();

            uint cookie;
            re.RegisterEditor(Arg.Any<Guid>(), Arg.Any<IVsEditorFactory>(), out cookie).ReturnsForAnyArgs(x => {
                cookie = 1;
                return VSConstants.S_OK;
            });

            re.UnregisterEditor(Arg.Any<uint>()).ReturnsForAnyArgs(VSConstants.S_OK);
            return re;
        }
    }
}
