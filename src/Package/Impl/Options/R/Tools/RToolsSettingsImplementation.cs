using System.ComponentModel.Composition;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.VisualStudio.R.Package.Options.R
{
    [Export(typeof(IRToolsSettings))]
    internal sealed class RToolsSettingsImplementation : IRToolsSettings
    {
        public string RVersion { get; set; } = Resources.Settings_RVersion_Latest;
        public string CranMirror { get; set; }

        public void LoadFromStorage()
        {
            using (var p = new RToolsOptionsPage())
            {
                p.LoadSettingsFromStorage();
            }
        }
    }
}
