namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class AttributesChanged : IFileSystemChange {
            private readonly string _name;
            private readonly string _fullPath;

            public AttributesChanged(string name, string fullPath) {
                _name = name;
                _fullPath = fullPath;
            }

            public void Apply(Changeset changeset) { }
        }
    }
}