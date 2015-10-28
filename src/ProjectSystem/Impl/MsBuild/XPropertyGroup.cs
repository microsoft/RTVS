using System.Xml.Linq;
using static Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild.XProjHelpers;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild {
    public class XPropertyGroup : XElement {
        public XPropertyGroup(params object[] elements) : base(MsBuildNamespace + "PropertyGroup", elements) { }

        public XPropertyGroup(string condition, params object[] elements)
            : base(MsBuildNamespace + "PropertyGroup", Content(elements, Attr("Condition", condition))) { }

        public XPropertyGroup(string label, string condition, params object[] elements)
            : base(MsBuildNamespace + "PropertyGroup", Content(elements, Attr("Label", label), Attr("Condition", condition))) { }
    }
}