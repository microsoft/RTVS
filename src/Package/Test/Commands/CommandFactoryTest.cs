using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.ContentType;

namespace Microsoft.VisualStudio.R.Package.Test.Commands {
    [ExcludeFromCodeCoverage]
    public class CommandFactoryTest
    {
        //[Test]
        //[Category.R.Package]
        public void Package_CommandFactoryImportTest()
        {
             var importComposer = new ContentTypeImportComposer<ICommandFactory>(EditorShell.Current.CompositionService);
            ICollection<ICommandFactory> factories = importComposer.GetAll(RContentTypeDefinition.ContentType);

            factories.Should().HaveCount(2);
        }
    }
}
