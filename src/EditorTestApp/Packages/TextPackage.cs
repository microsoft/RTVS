using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Settings;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Packages
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IWritableSettingsStorage))]
    [ContentType("text")]
    [Name("Generic Test settings")]
    [Order(Before = "Default")]
    internal sealed class TextSettingsStorage : SettingsStorage
    {
    }
}
