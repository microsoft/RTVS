using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Settings;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Packages
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IWritableEditorSettingsStorage))]
    [ContentType("text")]
    [Name("Generic Test settings")]
    [Order(Before = "Default")]
    internal sealed class TextSettingsStorage : SettingsStorage
    {
    }
}
