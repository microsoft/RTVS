using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Settings;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Packages
{
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [Name("HTML Text View Connection Listener")]
    [Order(Before = "Default")]
    internal sealed class TestRTextViewConnectionListener : RTextViewConnectionListener
    {
        public TestRTextViewConnectionListener()
        {
        }
    }

    [Export(typeof(IWritableEditorSettingsStorage))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Test settings")]
    [Order(Before = "Default")]
    internal sealed class RSettingsStorage : SettingsStorage
    {
    }
}
