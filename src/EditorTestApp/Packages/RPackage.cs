using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Settings;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Packages
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [Name("R Text View Connection Listener")]
    [Order(Before = "Default")]
    internal sealed class TestRTextViewConnectionListener : RTextViewConnectionListener
    {
        public TestRTextViewConnectionListener()
        {
        }
    }

    [ExcludeFromCodeCoverage]
    [Export(typeof(IWritableSettingsStorage))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Test settings")]
    [Order(Before = "Default")]
    internal sealed class RSettingsStorage : SettingsStorage
    {
    }
}
