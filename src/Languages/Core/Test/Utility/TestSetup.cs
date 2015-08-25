using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Languages.Core.Test.Utility
{
    [ExcludeFromCodeCoverage]
    public static class TestSetup
    {
        private static object _copyFilesLock = new object();

        public static void GetTestFolders(string editorRelativePath, string outputRelativePath, TestContext context, out string sourceFolder, out string destinationFolder)
        {
            string thisAssembly = Assembly.GetExecutingAssembly().Location;
            string assemblyLoc = Path.GetDirectoryName(thisAssembly);
            string enlistmentRoot = null;

            // The IDE deploys to TestResults (either as a sibling of the solution file or individual project files)
            int testResultsIndex = assemblyLoc.IndexOf(@"\TestResults\", StringComparison.OrdinalIgnoreCase);

            if (testResultsIndex >= 0)
            {
                int srcIndex = assemblyLoc.LastIndexOf(@"\test\", testResultsIndex, StringComparison.OrdinalIgnoreCase);
                enlistmentRoot = assemblyLoc.Substring(0, (srcIndex >= 0) ? srcIndex : testResultsIndex);
            }
            else if (assemblyLoc.ToLowerInvariant().IndexOf(@"\school\", StringComparison.OrdinalIgnoreCase) != -1)
            {
                // in this case, we're running from Maddog, so just take the current path
                enlistmentRoot = assemblyLoc;
            }
            else
            {
                // Running tests from the command line will deploy into the "bin" directory
                int binIndex = assemblyLoc.IndexOf(@"\bin\", StringComparison.OrdinalIgnoreCase);
                Assert.AreNotEqual(-1, binIndex);
                enlistmentRoot = assemblyLoc.Substring(0, binIndex) + "\\src"; // Git version
            }

            Assert.IsNotNull(enlistmentRoot);

            sourceFolder = Path.Combine(enlistmentRoot, editorRelativePath);
            destinationFolder = Path.Combine(context.TestRunDirectory, outputRelativePath);
        }

        public static void CopyDirectory(string src, string dst)
        {
            CopyDirectory(src, dst, "*");
        }

        public static void CopyDirectory(string src, string dst, string searchPattern)
        {
            lock (_copyFilesLock)
            {
                if (Directory.Exists(src))
                {
                    Directory.CreateDirectory(dst);

                    string[] dirs = Directory.GetDirectories(src);
                    foreach (string srcDir in dirs)
                    {
                        string srcDirName = Path.GetFileName(srcDir);
                        string dstDir = Path.Combine(dst, srcDirName);

                        CopyDirectory(srcDir, dstDir, searchPattern);
                    }

                    string[] srcFiles = Directory.GetFiles(src, searchPattern);
                    HashSet<string> expectedDestinationFileSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string filePath in srcFiles)
                    {
                        string fileName = Path.GetFileName(filePath);
                        string dstPath = Path.Combine(dst, fileName);
                        expectedDestinationFileSet.Add(dstPath);

                        if (!File.Exists(dstPath) || File.GetLastWriteTimeUtc(dstPath) < File.GetLastWriteTimeUtc(filePath))
                        {
                            File.Copy(filePath, dstPath, overwrite: true);
                            File.SetAttributes(dstPath, FileAttributes.Normal);
                        }
                    }

                    // In case we deleted some files from the source, make sure they don't stick around in the destination
                    HashSet<string> actualDestinationFileSet = new HashSet<string>(Directory.GetFiles(dst, searchPattern), StringComparer.OrdinalIgnoreCase);
                    actualDestinationFileSet.ExceptWith(expectedDestinationFileSet);

                    foreach (string staleFile in actualDestinationFileSet)
                    {
                        File.Delete(staleFile);
                    }
                }
            }
        }
    }
}
