using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.R.Package.Editors;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages.Markdown {
    [Guid(MdGuidList.MdEditorFactoryGuidString)]
    internal sealed class MdEditorFactory : BaseEditorFactory {
        public MdEditorFactory(Microsoft.VisualStudio.Shell.Package package) :
            base(package, MdGuidList.MdEditorFactoryGuid, MdGuidList.MdLanguageServiceGuid) { }

        public override int CreateEditorInstance(
            uint createEditorFlags,
            string documentMoniker,
            string physicalView,
            IVsHierarchy hierarchy,
            uint itemid,
            IntPtr docDataExisting,
            out IntPtr docView,
            out IntPtr docData,
            out string editorCaption,
            out Guid commandUIGuid,
            out int createDocumentWindowFlags) {
            return base.CreateEditorInstance(
                createEditorFlags,
                documentMoniker,
                physicalView,
                hierarchy,
                itemid,
                docDataExisting,
                out docView,
                out docData,
                out editorCaption,
                out commandUIGuid,
                out createDocumentWindowFlags);
        }
    }
}
