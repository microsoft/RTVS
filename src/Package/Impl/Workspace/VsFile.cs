using System;
using System.IO;
using Microsoft.Languages.Editor.Workspace;

namespace Microsoft.VisualStudio.R.Package.Workspace
{
    /// <summary>
    /// Visual Studio implementation of the file abstraction
    /// </summary>
    internal class VsFile : IFile
    {
        public VsFile(string absolutePath)
        {
            if (absolutePath == null)
                throw new ArgumentNullException("absolutePath");

            Name = Path.GetFileName(absolutePath);
            Folder = new VsFolder(Path.GetDirectoryName(absolutePath));
        }

        #region IFile

        public string Name { get; private set; }
        public IFolder Folder { get; private set; }

        #endregion
    }
}
