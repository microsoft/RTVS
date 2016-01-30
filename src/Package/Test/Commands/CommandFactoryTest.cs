using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.ContentType;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Commands {
    [ExcludeFromCodeCoverage]
    public class CommandFactoryTest
    {
        [Test]
        [Category.R.Package]
        public void CommandFactoryImportTest()
        {
            var importComposer = new ContentTypeImportComposer<ICommandFactory>(EditorShell.Current.CompositionService);
            ICollection<ICommandFactory> factories = importComposer.GetAll(RContentTypeDefinition.ContentType);
            factories.Should().HaveCount(1);

            importComposer = new ContentTypeImportComposer<ICommandFactory>(VsAppShell.Current.CompositionService);
            factories = importComposer.GetAll(RContentTypeDefinition.ContentType);
            factories.Should().HaveCount(2);
        }
    }
}
