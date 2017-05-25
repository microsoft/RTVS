// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class TextBufferUtilities {
        public static T GetBufferAdapter<T>(this ITextBuffer textBuffer, IServiceContainer services) where T : class {
            var adaptersFactoryService = services.GetService<IVsEditorAdaptersFactoryService>();
            var vsTextBuffer = adaptersFactoryService.GetBufferAdapter(textBuffer);
            if (vsTextBuffer == null) {
                var sp = services.GetService<IServiceProvider>();
                vsTextBuffer = adaptersFactoryService.CreateVsTextBufferAdapterForSecondaryBuffer(sp, textBuffer);
            }
            return vsTextBuffer as T;
        }

        public static ITextBuffer ToITextBuffer(this IVsTextBuffer vsTextBuffer, IServiceContainer services) 
            => services.GetService<IVsEditorAdaptersFactoryService>().GetDocumentBuffer(vsTextBuffer);

        public static ITextBuffer ToITextBuffer(this IVsTextLayer vsTextLayer, IServiceContainer services) {
            vsTextLayer.GetBaseBuffer(out IVsTextLines vsTextLines);
            return vsTextLines.ToITextBuffer(services);
        }

        public static ITextBuffer ToITextBuffer(this IVsTextLines vsTextLines, IServiceContainer services) => ToITextBuffer(vsTextLines as IVsTextBuffer, services);
    }
}
