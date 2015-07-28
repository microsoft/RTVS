using static System.FormattableString;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild
{
	public class XImportExisting : XImport
	{
		public XImportExisting(string project) : base(project, Invariant($"Exists('{project}')"))
		{
		}

		public XImportExisting(string project, string additionalCondition) : base(project, Invariant($"Exists('{project}') And {additionalCondition}"))
		{
		}
	}
}