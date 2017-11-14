// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IVsInteractiveWindowFactory))]
    [Export(typeof(IVsInteractiveWindowFactory2))]
    public sealed class VsInteractiveWindowFactoryMock : IVsInteractiveWindowFactory2 {
        public IVsInteractiveWindow Create(Guid providerId, int instanceId, string title, IInteractiveEvaluator evaluator, __VSCREATETOOLWIN creationFlags = 0) 
            => new VsInteractiveWindowMock(new WpfTextViewMock(new TextBufferMock(string.Empty, "R")), evaluator);

        public IVsInteractiveWindow Create(Guid providerId, int instanceId, string title, IInteractiveEvaluator evaluator, __VSCREATETOOLWIN creationFlags, Guid toolbarCommandSet, uint toolbarId,
            IOleCommandTarget toolbarCommandTarget)
            => new VsInteractiveWindowMock(new WpfTextViewMock(new TextBufferMock(string.Empty, "R")), evaluator);
    }
}