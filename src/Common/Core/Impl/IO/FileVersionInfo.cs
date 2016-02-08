namespace Microsoft.Common.Core.IO {
    internal class FileVersionInfo : IFileVersionInfo {
        public FileVersionInfo(int major, int minor) {
            FileMajorPart = major;
            FileMinorPart = minor;
        }
        public int FileMajorPart { get; }

        public int FileMinorPart { get; }
    }
}
