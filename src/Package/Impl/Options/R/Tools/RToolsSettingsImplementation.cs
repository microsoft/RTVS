using System.ComponentModel.Composition;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.VisualStudio.R.Package.Options.R
{
    [Export(typeof(IRToolsSettings))]
    internal sealed class RToolsSettingsImplementation : IRToolsSettings
    {
        public string GetRVersionPath()
        {
            // TODO: implement
            return string.Empty;
        }
    }
}
