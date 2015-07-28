using static System.FormattableString;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild
{
	public class XDefaultValueProperty : XProperty
	{
		public XDefaultValueProperty(string name, string defaultValue)
			: base(name, Invariant($"'$({name})' == ''"), defaultValue)
		{
		}
	}
}