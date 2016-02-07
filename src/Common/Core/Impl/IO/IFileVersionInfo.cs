namespace Microsoft.Common.Core.IO {
    public interface IFileVersionInfo {
        int FileMajorPart { get; }
        int FileMinorPart { get; }
    }
}
