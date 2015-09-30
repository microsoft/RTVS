using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.R.Package.Editors;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Commands
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RPackageTest : UnitTestBase
    {
        private static readonly IEnumerable<string> packageMefAssemblies = new string[]
        {
            "Microsoft.VisualStudio.R.Package.Test.dll"
        };

        [TestMethod]
        public void RPackage_ConstructionTest()
        {
            EditorShell.SetShell(TestEditorShell.Create(packageMefAssemblies));

            var package = new TestRPackage();
            package.Init();
        }

        //[TestMethod]
        public void RPackage_EditorFactoryTest()
        {
            EditorShell.SetShell(TestEditorShell.Create(packageMefAssemblies));

            var package = new TestRPackage();
            package.Init();

            IntPtr docView;
            IntPtr docData;
            string caption;
            Guid commandUiGuid;
            int flags;

            var editorFactory = new REditorFactory(package);
            editorFactory.CreateEditorInstance(VSConstants.CEF_OPENFILE, "file.r", string.Empty, null, 0, IntPtr.Zero,
                out docView, out docData, out caption, out commandUiGuid, out flags);
        }
    }
}
