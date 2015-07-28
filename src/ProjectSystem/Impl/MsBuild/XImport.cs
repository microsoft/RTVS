using System.Xml.Linq;
using static Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild.XProjHelpers;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild
{
	public class XImport : XElement
	{
		public XImport(string project) : base(MsBuildNamespace + "Import", Attr("Project", project))
		{
		}

		public XImport(string project, string condition) : base(MsBuildNamespace + "Import", Attr("Project", project), Attr("Condition", condition))
		{
		}
	}
}