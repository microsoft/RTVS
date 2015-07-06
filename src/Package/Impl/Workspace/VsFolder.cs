using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Languages.Editor.Workspace;

namespace Microsoft.VisualStudio.R.Package.Workspace
{
    /// <summary>
    /// Visual Studio implementation of the folder abstraction
    /// </summary>
    internal class VsFolder : IFolder
    {
        public VsFolder(string folder)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            Name = folder;
        }

        #region IWebFolder
        public string Name { get; private set; }

        public IFolder Parent
        {
            get
            {
                string parent = Path.GetDirectoryName(Name);
                if (String.IsNullOrEmpty(parent))
                    return null;

                return new VsFolder(parent);
            }
        }

        public ReadOnlyCollection<IFolder> Folders
        {
            get
            {
                var list = new List<IFolder>();

                if (Directory.Exists(Name))
                {
                    var folders = Directory.GetDirectories(Name);
                    foreach (var folder in folders)
                    {
                        list.Add(new VsFolder(folder));
                    }
                }

                return new ReadOnlyCollection<IFolder>(list);
            }
        }

        public ReadOnlyCollection<IFile> Files
        {
            get
            {
                var list = new List<IFile>();

                if (Directory.Exists(Name))
                {
                    var files = Directory.GetFiles(Name);
                    foreach (var file in files)
                    {
                        list.Add(new VsFile(file));
                    }
                }

                return new ReadOnlyCollection<IFile>(list);
            }
        }
        #endregion
    }
}
