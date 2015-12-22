using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Host.Client.Test {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class GraphicsDeviceTestFilesSetup {
        static object _deploymentLock = new object();
        static bool _deployed = false;

        [AssemblyInitialize]
        public static void DeployFiles(TestContext context) {
            lock (_deploymentLock) {
                if (!_deployed) {
                    _deployed = true;

                    string srcFilesFolder;
                    string testFilesDir;

                    TestSetupUtilities.GetTestFolders(@"Host\Client\Test\Files", CommonTestData.TestFilesRelativePath, context.TestRunDirectory, out srcFilesFolder, out testFilesDir);
                    TestSetupUtilities.CopyDirectory(srcFilesFolder, testFilesDir);
                }
            }
        }
    }
}
