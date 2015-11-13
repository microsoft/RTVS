using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Imaging;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
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
        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource folderGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphClosedFolder, StandardGlyphItem.GlyphItemPublic);
            string currentDir = RToolsSettings.Current.WorkingDirectory;
            string directory;

            try {
                directory = Path.Combine(currentDir, _directory);
                if (!Directory.Exists(directory)) {
                    directory = currentDir;
                }
            } catch (IOException) {
                directory = currentDir;
            }

            try {
                foreach (string dir in Directory.GetDirectories(directory)) {
                    string dirName = Path.GetFileName(dir);
                    completions.Add(new RCompletion(dirName, dirName + "/", string.Empty, folderGlyph));
                }

                foreach (string file in Directory.GetFiles(directory)) {
                    ImageSource fileGlyph = GetImageForFile(file);
                    string fileName = Path.GetFileName(file);
                    completions.Add(new RCompletion(fileName, fileName, string.Empty, fileGlyph));
                }
            } catch (IOException) { }

            return completions;
        }
        #endregion

        private ImageSource GetImageForFile(string name) {
            string ext = Path.GetExtension(name);
            if (ext == ".R" || ext == ".r") {
                return ImagesProvider.GetImage("RFileNode");
            }
            if (ext.Equals(".rproj", StringComparison.OrdinalIgnoreCase)) {
                return ImagesProvider.GetImage("RProjectNode");
            }
            if (ext.Equals(".rdata", StringComparison.OrdinalIgnoreCase)) {
                return ImagesProvider.GetImage("RDataNode");
            }
            if (ext.Equals(".md", StringComparison.OrdinalIgnoreCase)) {
                return ImagesProvider.GetImage("MarkdownFile");
            }
            if (ext.Equals(".rmd", StringComparison.OrdinalIgnoreCase)) {
                return ImagesProvider.GetImage("MarkdownFile");
            }
            if (ext.Equals(".html", StringComparison.OrdinalIgnoreCase)) {
                return ImagesProvider.GetImage("HTMLFile");
            }
            if (ext.Equals(".css", StringComparison.OrdinalIgnoreCase)) {
                return ImagesProvider.GetImage("StyleSheet");
            }
            if (ext.Equals(".xml", StringComparison.OrdinalIgnoreCase)) {
                return ImagesProvider.GetImage("XMLFile");
            }
            if (ext.Equals(".txt", StringComparison.OrdinalIgnoreCase)) {
                return ImagesProvider.GetImage("TextFile");
            }
            return ImagesProvider.GetImage("Document");
        }

        private string ExtractDirectory(string directory) {
            if (directory.Length > 0 && (directory[0] == '\"' || directory[0] == '\'')) {
                directory = directory.Substring(1);
            }
            if (directory.Length > 0 && (directory[directory.Length - 1] == '\"' || directory[directory.Length - 1] == '\'')) {
                directory = directory.Substring(0, directory.Length - 1);
            }
            return directory.Replace('/', '\\');
        }
    }
}
