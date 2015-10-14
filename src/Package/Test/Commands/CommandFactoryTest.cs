using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Commands
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class CommandFactoryTest : UnitTestBase
    {
        //[TestMethod]
        public void Package_CommandFactoryImportTest()
        {
            EditorShell.SetShell(TestEditorShell.Create(RPackageTestCompositionCatalog.Current));

            var importComposer = new ContentTypeImportComposer<ICommandFactory>(EditorShell.Current.CompositionService);
            ICollection<ICommandFactory> factories = importComposer.GetAll(RContentTypeDefinition.ContentType);

            Assert.AreEqual(2, factories.Count);
        }
    }
}
