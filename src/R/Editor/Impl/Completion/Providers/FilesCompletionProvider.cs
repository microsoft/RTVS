using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Completions.Definitions;
using Microsoft.R.Editor.Imaging;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// Provides list of files and folder in the current directory
    /// </summary>
    public class FilesCompletionProvider : IRCompletionListProvider {
        [Import]
        private IImagesProvider ImagesProvider { get; set; }

        private string _directory;
        public FilesCompletionProvider(string directoryCandidate) {
            EditorShell.Current.CompositionService.SatisfyImportsOnce(this);
            _directory = ExtractDirectory(directoryCandidate);
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = false;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource folderGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphClosedFolder, StandardGlyphItem.GlyphItemPublic);
            string currentDir = RToolsSettings.Current.WorkingDirectory;
            string directory = null;

            try {
                string dir = Path.Combine(currentDir, _directory);
                if (Directory.Exists(dir)) {
                    directory = dir;
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { } catch (ArgumentException) { }

            if (directory != null) {
                try {
                    foreach (string dir in Directory.GetDirectories(directory)) {
                        DirectoryInfo di = new DirectoryInfo(dir);
                        if (!di.Attributes.HasFlag(FileAttributes.Hidden) && !di.Attributes.HasFlag(FileAttributes.System)) {
                            string dirName = Path.GetFileName(dir);
                            completions.Add(new RCompletion(dirName, dirName + "/", string.Empty, folderGlyph));
                        }
                    }

                    foreach (string file in Directory.GetFiles(directory)) {
                        FileInfo di = new FileInfo(file);
                        if (!di.Attributes.HasFlag(FileAttributes.Hidden) && !di.Attributes.HasFlag(FileAttributes.System)) {
                            ImageSource fileGlyph = ImagesProvider.GetFileIcon(file);
                            string fileName = Path.GetFileName(file);
                            completions.Add(new RCompletion(fileName, fileName, string.Empty, fileGlyph));
                        }
                    }
                } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }

            return completions;
        }
        #endregion

        private string ExtractDirectory(string directory) {
            if(directory.Length == 0) {
                return string.Empty;
            }
            if (directory[0] == '\"' || directory[0] == '\'') {
                directory = directory.Substring(1);
            }
            if (directory[directory.Length - 1] == '\"' || directory[directory.Length - 1] == '\'') {
                directory = directory.Substring(0, directory.Length - 1);
            }
            return directory.Replace('/', '\\');
        }
    }
}
