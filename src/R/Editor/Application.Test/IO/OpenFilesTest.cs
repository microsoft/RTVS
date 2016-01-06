using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.IO {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class OpenFilesTest
    {
        private readonly EditorAppTestFilesFixture _files;

        public OpenFilesTest(EditorAppTestFilesFixture files) {
            _files = files;
        }

        [Test(Skip="Unstable")]
        [Category.Interactive]
        public void OpenFile_R()
        {
            OpenFileInEditor("lsfit.r");
        }

        [Test(Skip = "Unstable")]
        [Category.Interactive]
        public void OpenFile_RD()
        {
            OpenFileInEditor("01.rd");
        }

        /// <summary>
        /// Opens file in an editor window
        /// </summary>
        /// <param name="fileName">File name</param>
        void OpenFileInEditor(string fileName)
        {
            string text = _files.LoadDestinationFile(fileName);

            try
            {
                EditorWindow.Create(text, fileName);
                EditorWindow.DoIdle(1000);
            }
            finally
            {
                EditorWindow.Close();
            }
        }
    }
}
