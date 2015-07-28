using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project
{
    public abstract class FileSystemMirroringProjectFileGenerator : IVsProjectGenerator
    {
        private readonly Guid _projectType;
	    private readonly string _projectUiSubcaption;
	    private readonly IEnumerable<string> _msBuildImports;

	    protected FileSystemMirroringProjectFileGenerator(Guid projectType, string projectUiSubcaption, IEnumerable<string> msBuildImports)
	    {
		    _projectType = projectType;
		    _projectUiSubcaption = projectUiSubcaption;
		    _msBuildImports = msBuildImports;
	    }

	    public void RunGenerator(string szSourceFileMoniker, out bool pfProjectIsGenerated, out string pbstrGeneratedFile, out Guid pGuidProjType)
        {
            pfProjectIsGenerated = true;
            pbstrGeneratedFile = GetCpsProjFileName(szSourceFileMoniker);
            pGuidProjType = _projectType;

            EnsureCpsProjFile(pbstrGeneratedFile);
        }

        private static string GetCpsProjFileName(string fileName)
        {
            return fileName + ".msbuild";
        }

        private void EnsureCpsProjFile(string cpsProjFileName)
        {
            // TODO: Find best way to deal with the file.
            // What if such file exists, but it is not our MSBuild file?
            // Maybe it is possible to remove it after project is created. In this case, name can be random
            var fileInfo = new FileInfo(cpsProjFileName);

            var vsVersion = "14.0";
            var inMemoryTargetsFile = GetInMemoryTargetsFileName(cpsProjFileName);

			var xProjDocument = new XProjDocument(
				new XProject(vsVersion, "Build",
					new XPropertyGroup("Globals", null,
						new XProperty("ProjectGuid", Guid.NewGuid().ToString("D"))
					),
					new XPropertyGroup(
						new XDefaultValueProperty("VisualStudioVersion", vsVersion),
						new XDefaultValueProperty("Configuration", "Debug"),
						new XDefaultValueProperty("Platform", "AnyCPU")
					),
					new XPropertyGroup(
						new XProperty("ProjectUISubcaption", _projectUiSubcaption)
					),
					new XProjElement("ProjectExtensions",
                        new XProjElement("VisualStudio",
							new XProjElement("UserProperties")
						)
					),
					_msBuildImports.SelectMany(CreateMsBuildExtensionXImports),
					new XImportExisting(inMemoryTargetsFile)
				)
			);

			using (var writer = fileInfo.CreateText())
			{
				xProjDocument.Save(writer);
			}
        }

	    public static string GetInMemoryTargetsFileName(string cpsProjFileName)
	    {
		    return Path.GetFileNameWithoutExtension(cpsProjFileName) + ".InMemory.Targets";
	    }

	    private IEnumerable<XImport> CreateMsBuildExtensionXImports(string import)
        {
            var msBuildImportExtensionPath = Invariant($@"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\{import}");
            var msBuildImportUserExtensionPath = Invariant($@"$(MSBuildUserExtensionsPath)\Microsoft\VisualStudio\v$(VisualStudioVersion)\{import}");

            yield return new XImportExisting(msBuildImportUserExtensionPath);
            yield return new XImportExisting(msBuildImportExtensionPath, $"!Exists('{msBuildImportUserExtensionPath}')");
        }
    }
}