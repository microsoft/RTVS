using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Commands {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RPackageTest : UnitTestBase {
       // [TestMethod]
        //public void RPackage_ConstructionTest() {
        //    SequentialEditorTestExecutor.ExecuteTest((ManualResetEventSlim evt) => {
        //        var package = new TestRPackage();
        //        package.Init();
        //        package.Close();

        //        evt.Set();
        //    }, RPackageTestCompositionCatalog.Current);
        //}

        //[TestMethod]
        //public void RPackage_EditorFactoryTest() {
        //    SequentialEditorTestExecutor.ExecuteTest((ManualResetEventSlim evt) => {
        //        var package = new TestRPackage();
        //        package.Init();

        //        IntPtr docView;
        //        IntPtr docData;
        //        string caption;
        //        Guid commandUiGuid;
        //        int flags;

        //        var editorFactory = new REditorFactory(package);
        //        editorFactory.InstanceFactory = new TestInstanceFactory();

        //        editorFactory.CreateEditorInstance(VSConstants.CEF_OPENFILE, "file.r", string.Empty, null, 0, IntPtr.Zero,
        //            out docView, out docData, out caption, out commandUiGuid, out flags);

        //        package.Close();

        //        evt.Set();
        //    }, RPackageTestCompositionCatalog.Current);
        //}
    }
}
