using System.ComponentModel.Composition;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.VisualStudio.R.Package.Options.R
{
    [Export(typeof(IRToolsSettings))]
    internal sealed class RToolsSettingsImplementation : IRToolsSettings
    {
        public string RVersionPath { get; set; } = string.Empty;
        public string CranMirror { get; set; }
    }
}
