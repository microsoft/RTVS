using System.Collections.Generic;

namespace Microsoft.Common.Core.IO
{
	public interface IDirectoryInfo : IFileSystemInfo
	{
        IDirectoryInfo Parent { get; }

        IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos();
	}
}