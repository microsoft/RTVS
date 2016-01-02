using System;
using Microsoft.Common.Core.Tests.Utility;

namespace Microsoft.Common.Core.Testss.Utility {
    public sealed class TestFilesSetupFixture: IDisposable {
        static readonly object _deploymentLock = new object();
        static bool _deployed = false;

        public void Initialize(string testFilePath) {
            lock (_deploymentLock) {
                if (!_deployed) {
                    _deployed = true;

                    string srcFilesFolder;
                    string testFilesDir;

                    TestSetupUtilities.GetTestFolders(testFilePath, CommonTestData.TestFilesRelativePath, "foo", out srcFilesFolder, out testFilesDir);
                    TestSetupUtilities.CopyDirectory(srcFilesFolder, testFilesDir);
                }
            }
        }

        public void Dispose() {
        }
    }
}
