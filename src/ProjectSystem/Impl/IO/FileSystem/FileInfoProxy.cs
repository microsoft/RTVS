using System.IO;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO.FileSystem
{
	internal class FileInfoProxy : IFileInfo
	{
		private readonly FileInfo _fileInfo;

		public FileInfoProxy(FileInfo fileInfo)
		{
			_fileInfo = fileInfo;
		}

		public bool Exists => _fileInfo.Exists;
		public string FullName => _fileInfo.FullName;
		public FileAttributes Attributes => _fileInfo.Attributes;
	}
}