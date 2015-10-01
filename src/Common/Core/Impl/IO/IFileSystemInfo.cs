using System.IO;

namespace Microsoft.Common.Core.IO
{
	public interface IFileSystemInfo
	{
		bool Exists { get; }
		string FullName { get; }
		FileAttributes Attributes { get; }

	    void Delete();
	}
}