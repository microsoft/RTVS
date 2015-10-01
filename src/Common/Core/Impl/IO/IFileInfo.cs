using System.IO;

namespace Microsoft.Common.Core.IO
{
	public interface IFileInfo : IFileSystemInfo
	{
        IDirectoryInfo Directory { get; }

        StreamWriter CreateText();
	}
}